using NPOI.SS.Formula.Functions;
using OrderProcessing.Classes;
using OrderProcessing.Enums;
using OrderProcessing.Repostries;

namespace OrderProcessing;

public class OrderValidator
{
    private readonly CustomerRepository _customerRepo;
    private readonly ProductRepository _productRepo;
    private readonly CouponRepository _couponRepo;
    private readonly OrderRepository _orderRepo;
    private const string ClassName = nameof(OrderValidator);

    public OrderValidator(CustomerRepository customerRepo, ProductRepository productRepo, CouponRepository couponRepo, OrderRepository orderRepo)
    {
        _customerRepo = customerRepo;
        _productRepo = productRepo;
        _couponRepo = couponRepo;
        _orderRepo = orderRepo;
    }
    public async Task<bool> ValidateOrderAsync(Order order, CancellationToken ct = default)
    {
        var customer = await _customerRepo.GetByIdAsync(order.CustomerId, ct);
        if(customer == null)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = $"Customer #{order.CustomerId} not found";
            await _orderRepo.SaveAsync(order, ct);
            Log.Error(ClassName, order.FailureReason);
            return false;
        }
        if (!customer.IsActive)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = "Customer is inactive";
            await _orderRepo.SaveAsync(order, ct);
            Log.Error(ClassName, order.FailureReason);
            return false;
        }

        if(order.Items != null)
        {
            foreach(var item in order.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId, ct);
                if(product == null)
                {
                    order.Status = OrderStatus.Rejected;
                    order.FailureReason = $"Product #{item.ProductId} not found";
                   await _orderRepo.SaveAsync(order, ct);
                    Log.Error(ClassName, order.FailureReason);
                    return false;
                }
                if (product.StockQuantity < item.Quantity)
                {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = $"Insufficient stock for '{product.Name}'";
                await _orderRepo.SaveAsync(order, ct);
                Log.Error(ClassName, order.FailureReason);
                return false;
                }
            }

        }
        if (!string.IsNullOrEmpty(order.CouponCode))
        {
            var coupon = await _couponRepo.GetByCodeAsync(order.CouponCode, ct);
            if (coupon == null)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = $"Coupon '{order.CouponCode}' does not exist";
                await _orderRepo.SaveAsync(order, ct);
                Log.Error(ClassName, order.FailureReason);
                return false;
            }

            if (coupon.ExpiryDate < DateTime.Today)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = $"Coupon '{order.CouponCode}' has expired";
                await _orderRepo.SaveAsync(order, ct);
                Log.Error(ClassName, order.FailureReason);
                return false;
            }
        }
        order.Status = OrderStatus.Validated;
        order.FailureReason = null;
        await _orderRepo.SaveAsync(order, ct);
        Log.Success(ClassName, $"Order #{order.Id} validated successfully.");
        return true;
    }
}
