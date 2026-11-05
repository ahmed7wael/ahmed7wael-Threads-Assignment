using OrderProcessing.Classes;
using OrderProcessing.Enums;

namespace OrderProcessing.interfaces;

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(int orderId, decimal amount, CancellationToken ct = default);
    Task RefundAsync(string transactionId, CancellationToken ct = default);

    PaymentResult ProcessPaymentSync(int orderId, decimal amount);
    void RefundSync(string transactionId); // <-- تأكد من وجودها هنا
}