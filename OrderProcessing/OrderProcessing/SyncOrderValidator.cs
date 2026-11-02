using OrderProcessing.Classes;
using OrderProcessing.Enums;
using OrderProcessing.Repostries;

namespace OrderProcessing;

public class SyncOrderValidator
{
    private readonly SyncCustomerRepository _customerRepo;
    private readonly SyncProductRepository _productRepo;
    private readonly SyncCouponRepository _couponRepo;
    private const string ClassName = nameof(SyncOrderValidator);

    public SyncOrderValidator(
        SyncCustomerRepository customerRepo,
        SyncProductRepository productRepo,
        SyncCouponRepository couponRepo)
    {
        _customerRepo = customerRepo;
        _productRepo = productRepo;
        _couponRepo = couponRepo;
    }

    public bool ValidateOrder(Order order, CancellationToken ct = default)
    {
        var customer = _customerRepo.GetById(order.CustomerId, ct);
        if (customer == null)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = $"Customer #{order.CustomerId} not found";
            Log.Error(ClassName, order.FailureReason);
            return false;
        }
        if (!customer.IsActive)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = "Customer is inactive";
            Log.Error(ClassName, order.FailureReason);
            return false;
        }

        if (order.Items != null)
        {
            foreach (var item in order.Items)
            {
                var product = _productRepo.GetById(item.ProductId, ct);
                if (product == null)
                {
                    order.Status = OrderStatus.Rejected;
                    order.FailureReason = $"Product #{item.ProductId} not found";
                    Log.Error(ClassName, order.FailureReason);
                    return false;
                }
                if (product.StockQuantity < item.Quantity)
                {
                    order.Status = OrderStatus.Rejected;
                    order.FailureReason = $"Insufficient stock for '{product.Name}'";
                    Log.Error(ClassName, order.FailureReason);
                    return false;
                }
            }
        }

        if (!string.IsNullOrEmpty(order.CouponCode))
        {
            var coupon = _couponRepo.GetByCode(order.CouponCode, ct);
            if (coupon == null)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = $"Coupon '{order.CouponCode}' does not exist";
                Log.Error(ClassName, order.FailureReason);
                return false;
            }

            if (coupon.ExpiryDate < DateTime.Today)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = $"Coupon '{order.CouponCode}' has expired";
                Log.Error(ClassName, order.FailureReason);
                return false;
            }
        }

        order.Status = OrderStatus.Validated;
        order.FailureReason = null;
        Log.Success(ClassName, $"Order #{order.Id} validated successfully.");
        return true;
    }
}