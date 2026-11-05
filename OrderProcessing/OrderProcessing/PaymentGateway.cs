using OrderProcessing.Classes;
using OrderProcessing.Enums;
using OrderProcessing.interfaces;

namespace OrderProcessing;

public class PaymentGateway : IPaymentGateway
{
    private const string ClassName = nameof(PaymentGateway);
    private readonly JsonFileStore _jsonFileStore;
    private readonly HashSet<int> _timeoutOrderIds;
    private readonly HashSet<int> _declinedOrderIds;
    private static readonly object _fileLock = new();

    public PaymentGateway(JsonFileStore jsonFileStore, IEnumerable<int>? timeoutOrderIds = null, IEnumerable<int>? declinedOrderIds = null)
    {
        _jsonFileStore = jsonFileStore;
        _timeoutOrderIds = timeoutOrderIds != null ? new HashSet<int>(timeoutOrderIds) : new HashSet<int> { 5 };
        _declinedOrderIds = declinedOrderIds != null ? new HashSet<int>(declinedOrderIds) : new HashSet<int> { 8 };
    }

    public async Task<PaymentResult> ProcessPaymentAsync(int orderId, decimal amount, CancellationToken ct = default)
    {
        Log.Info(ClassName, $"Authorizing payment for Order #{orderId}...");
        await Task.Delay(200, ct);

        if (_declinedOrderIds.Contains(orderId))
        {
            string txnId = "TXN-" + Guid.NewGuid().ToString("N")[..12].ToUpper();
            var declinedTransaction = new PaymentTransaction
            {
                OrderId = orderId,
                Amount = amount,
                Status = PaymentStatus.Declined,
                TransactionId = txnId,
                CreatedAt = DateTime.UtcNow
            };

            lock (_fileLock)
            {
                var payments = _jsonFileStore.Read<PaymentTransaction>(DataFiles.payments) ?? new List<PaymentTransaction>();
                payments.Add(declinedTransaction);
                _jsonFileStore.Write(DataFiles.payments, payments);
            }

            return new PaymentResult { Success = false, TransactionId = txnId, Status = PaymentStatus.Declined, Message = "Card declined by issuer" };
        }

        if (_timeoutOrderIds.Contains(orderId))
        {
            await Task.Delay(4000, ct);
            return new PaymentResult { Success = false, Status = PaymentStatus.Declined, Message = "Payment timeout" };
        }

        await Task.Delay(200, ct);
        string transactionId = "TXN-" + Guid.NewGuid().ToString("N")[..12].ToUpper();

        var transaction = new PaymentTransaction
        {
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Captured,
            TransactionId = transactionId,
            CreatedAt = DateTime.UtcNow
        };

        lock (_fileLock)
        {
            var payments = _jsonFileStore.Read<PaymentTransaction>(DataFiles.payments) ?? new List<PaymentTransaction>();
            payments.Add(transaction);
            _jsonFileStore.Write(DataFiles.payments, payments);
        }

        return new PaymentResult { Success = true, TransactionId = transactionId, Status = PaymentStatus.Captured, Message = "Payment processed successfully." };
    }

    public async Task RefundAsync(string transactionId, CancellationToken ct = default)
    {
        RefundSync(transactionId);
        await Task.CompletedTask;
    }

    public PaymentResult ProcessPaymentSync(int orderId, decimal amount)
    {
        Thread.Sleep(200);

        if (_declinedOrderIds.Contains(orderId))
        {
            string txnId = "TXN-" + Guid.NewGuid().ToString("N")[..12].ToUpper();
            var declinedTransaction = new PaymentTransaction
            {
                OrderId = orderId,
                Amount = amount,
                Status = PaymentStatus.Declined,
                TransactionId = txnId,
                CreatedAt = DateTime.UtcNow
            };

            lock (_fileLock)
            {
                var payments = _jsonFileStore.Read<PaymentTransaction>(DataFiles.payments) ?? new List<PaymentTransaction>();
                payments.Add(declinedTransaction);
                _jsonFileStore.Write(DataFiles.payments, payments);
            }

            return new PaymentResult { Success = false, TransactionId = txnId, Status = PaymentStatus.Declined, Message = "Card declined by issuer" };
        }

        if (_timeoutOrderIds.Contains(orderId))
        {
            Thread.Sleep(4000);
            return new PaymentResult { Success = false, Status = PaymentStatus.Declined, Message = "Payment timeout" };
        }

        Thread.Sleep(200);
        string transactionId = "TXN-" + Guid.NewGuid().ToString("N")[..12].ToUpper();
        var transaction = new PaymentTransaction
        {
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Captured,
            TransactionId = transactionId,
            CreatedAt = DateTime.UtcNow
        };

        lock (_fileLock)
        {
            var payments = _jsonFileStore.Read<PaymentTransaction>(DataFiles.payments) ?? new List<PaymentTransaction>();
            payments.Add(transaction);
            _jsonFileStore.Write(DataFiles.payments, payments);
        }

        return new PaymentResult { Success = true, TransactionId = transactionId, Status = PaymentStatus.Captured, Message = "Payment processed successfully." };
    }

    public void RefundSync(string transactionId)
    {
        lock (_fileLock)
        {
            var transactions = _jsonFileStore.Read<PaymentTransaction>(DataFiles.payments);
            if (transactions != null)
            {
                foreach (var tx in transactions)
                {
                    if (tx.TransactionId == transactionId)
                        tx.Status = PaymentStatus.Refunded;
                }
                _jsonFileStore.Write(DataFiles.payments, transactions);
            }
        }
    }
}