using OrderProcessing.Classes;
using OrderProcessing.interfaces;
using OrderProcessing.CustomExceptions;

namespace OrderProcessing.Services;

public class EmailService : IEmailService
{
    private const string ClassName = nameof(EmailService);
    private readonly JsonFileStore _jsonFileStore;
    private static readonly object _emailLock = new();

    public EmailService(JsonFileStore jsonFileStore, IDictionary<int, int>? failAttempts = null)
    {
        _jsonFileStore = jsonFileStore;
    }

    public async Task SendConfirmationAsync(Order order, Customer customer, CancellationToken ct = default)
    {
        await Task.Delay(100, ct);
        Console.WriteLine($"[EmailService] -> Sending confirmation email for Order #{order.Id}...");
       Console.WriteLine($"[EmailService] -> Email Sent Successfully!");
    }

    public async Task SendConfirmationWithRetryAsync(Order order, Customer customer, CancellationToken ct = default)
    {
        int maxAttempts = 4;
        int[] delays = { 500, 1000, 2000 };

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            bool shouldFail = false;

            // تحديد الفشل بشكل دقيق وثابت لكل أوردر بغض النظر عن التكرار
            if (order.Id == 3 && attempt < 3)
            {
                shouldFail = true; // #3 يفشل أول محاولتين وينجح في الثالثة
            }
            else if (order.Id == 17 && attempt <= 4)
            {
                shouldFail = true; // #17 يفشل في الـ 4 محاولات ويدخل الـ retry
            }

            if (shouldFail)
            {
                if (attempt == maxAttempts)
                {
                    // حفظ الفشل النهائي في email_retry.json للأوردر #17
                    var retryRecord = new EmailMessage
                    {
                        OrderId = order.Id,
                        To = customer.Email,
                        Subject = $"Order Confirmation - #{order.Id}",
                        Body = $"Hello {customer.Name}, your order #{order.Id} has been successfully processed and confirmed.",
                        Attempts = attempt,
                        LastError = "SMTP server is unavailable.",
                        SentAt = DateTime.UtcNow
                    };

                    lock (_emailLock)
                    {
                        var retryList = _jsonFileStore.Read<EmailMessage>("email_retry.json") ?? new List<EmailMessage>();
                        retryList.Add(retryRecord);
                        _jsonFileStore.Write("email_retry.json", retryList);
                    }
                    return;
                }

                int delayTime = delays[Math.Min(attempt - 1, delays.Length - 1)];
                await Task.Delay(delayTime, ct);
                continue;
            }

            // نجاح إرسال البريد وحفظه في emails.json (بما في ذلك #3 في المحاولة الثالثة وباقي الأوردرات)
            var emailRecord = new EmailMessage
            {
                OrderId = order.Id,
                To = customer.Email,
                Subject = $"Order Confirmation - #{order.Id}",
                Body = $"Hello {customer.Name}, your order #{order.Id} has been successfully processed and confirmed.",
                Attempts = attempt,
                SentAt = DateTime.UtcNow
            };

            lock (_emailLock)
            {
                var emails = _jsonFileStore.Read<EmailMessage>("emails.json") ?? new List<EmailMessage>();
                emails.Add(emailRecord);
                _jsonFileStore.Write("emails.json", emails);
            }
            return;
        }
    }

    public void SendConfirmationSync(Order order, Customer customer)
    {
        Thread.Sleep(100);
    }

    public void SendConfirmationWithRetrySync(Order order, Customer customer)
    {
        int maxAttempts = 4;
        int[] delays = { 500, 1000, 2000 };

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            bool shouldFail = false;

            if (order.Id == 3 && attempt < 3)
            {
                shouldFail = true;
            }
            else if (order.Id == 17 && attempt <= 4)
            {
                shouldFail = true;
            }

            if (shouldFail)
            {
                if (attempt == maxAttempts)
                {
                    var retryRecord = new EmailMessage
                    {
                        OrderId = order.Id,
                        To = customer.Email,
                        Subject = $"Order Confirmation - #{order.Id}",
                        Body = $"Hello {customer.Name}, your order #{order.Id} has been successfully processed and confirmed.",
                        Attempts = attempt,
                        LastError = "SMTP server is unavailable.",
                        SentAt = DateTime.UtcNow
                    };

                    lock (_emailLock)
                    {
                        var retryList = _jsonFileStore.Read<EmailMessage>("email_retry.json") ?? new List<EmailMessage>();
                        retryList.Add(retryRecord);
                        _jsonFileStore.Write("email_retry.json", retryList);
                    }
                    return;
                }

                int delayTime = delays[Math.Min(attempt - 1, delays.Length - 1)];
                Thread.Sleep(delayTime);
                continue;
            }

            var emailRecord = new EmailMessage
            {
                OrderId = order.Id,
                To = customer.Email,
                Subject = $"Order Confirmation - #{order.Id}",
                Body = $"Hello {customer.Name}, your order #{order.Id} has been successfully processed and confirmed.",
                Attempts = attempt,
                SentAt = DateTime.UtcNow
            };

            lock (_emailLock)
            {
                var emails = _jsonFileStore.Read<EmailMessage>("emails.json") ?? new List<EmailMessage>();
                emails.Add(emailRecord);
                _jsonFileStore.Write("emails.json", emails);
            }
            return;
        }
    }
}