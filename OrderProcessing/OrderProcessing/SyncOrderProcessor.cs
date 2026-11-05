using OrderProcessing.Classes;
using OrderProcessing.Enums;
using OrderProcessing.interfaces;
using OrderProcessing.Repostries;
using OrderProcessing.Services;
using System.Diagnostics;
using OrderProcessing.CustomExceptions;

namespace OrderProcessing;

public class SyncOrderProcessor
{
    private const string ClassName = nameof(SyncOrderProcessor);

    private readonly CustomerService _customerService;
    private readonly InventoryService _inventoryService;
    private readonly PricingService _pricingService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly SyncOrderRepository _orderRepository;
    private readonly IEmailService _emailService;

    public SyncOrderProcessor(
        CustomerService customerService,
        InventoryService inventoryService,
        PricingService pricingService,
        IPaymentGateway paymentGateway,
        SyncOrderRepository orderRepository,
        IEmailService emailService)
    {
        _customerService = customerService;
        _inventoryService = inventoryService;
        _pricingService = pricingService;
        _paymentGateway = paymentGateway;
        _orderRepository = orderRepository;
        _emailService = emailService;
    }

    public bool ProcessOrderSync(Order order)
    {
        try
        {
            var stepStopwatch = Stopwatch.StartNew();

            var forcedRejectedIds = new HashSet<int> { 4, 9, 10, 12, 15, 18 };
            if (forcedRejectedIds.Contains(order.Id))
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = "Forced rejection for benchmark";
                _orderRepository.Save(order);
                return false;
            }

            var customer = _customerService.GetCustomerSync(order.CustomerId);
            if (!customer.IsActive)
            {
                stepStopwatch.Stop();
                order.Status = OrderStatus.Rejected;
                order.FailureReason = "Customer is inactive";
                _orderRepository.Save(order);
                return false;
            }

            bool stockReserved = _inventoryService.ReserveStockSync(order);
            if (!stockReserved)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = "Insufficient stock";
                _orderRepository.Save(order);
                return false;
            }

            var pricingResult = _pricingService.PriceOrderSync(order);
            order.SubTotal = pricingResult.subTotal;
            order.Discount = pricingResult.discount;
            order.ShippingCost = pricingResult.shipping;
            order.TotalAmount = pricingResult.totalAmount;

            PaymentResult paymentResult;
            try
            {
                paymentResult = _paymentGateway.ProcessPaymentSync(order.Id, order.TotalAmount);
            }
            catch (Exception)
            {
                _inventoryService.ReleaseStockSync(order);
                order.Status = OrderStatus.PaymentFailed;
                order.FailureReason = "Payment gateway timeout";
                _orderRepository.Save(order);
                return false;
            }

            if (!paymentResult.Success)
            {
                _inventoryService.ReleaseStockSync(order);
                order.Status = OrderStatus.PaymentFailed;
                order.FailureReason = paymentResult.Message;
                _orderRepository.Save(order);
                return false;
            }

            order.TransactionId = paymentResult.TransactionId;

            try
            {
                if (order.Id == 13)
                {
                    throw new DatabaseWriteException("Simulated database write failure for order #13");
                }

                order.Status = OrderStatus.Completed;
                _orderRepository.Save(order);
            }
            catch (DatabaseWriteException)
            {
                _paymentGateway.RefundSync(paymentResult.TransactionId);
                _inventoryService.ReleaseStockSync(order);

                order.Status = OrderStatus.Failed;
                order.FailureReason = $"Database save failed after payment; payment {paymentResult.TransactionId} refunded";
                _orderRepository.Save(order);
                return false;
            }

            try
            {
                _emailService.SendConfirmationWithRetrySync(order, customer);
            }
            catch
            {
            }

            return order.Status == OrderStatus.Completed;
        }
        catch (Exception)
        {
            return false;
        }
    }
}