using OrderProcessing.Classes;

namespace OrderProcessing.interfaces;

public interface IEmailService
{
    Task SendConfirmationAsync(Order order, Customer customer, CancellationToken ct = default);
    Task SendConfirmationWithRetryAsync(Order order, Customer customer, CancellationToken ct = default);

    void SendConfirmationSync(Order order, Customer customer);
    void SendConfirmationWithRetrySync(Order order, Customer customer);
}