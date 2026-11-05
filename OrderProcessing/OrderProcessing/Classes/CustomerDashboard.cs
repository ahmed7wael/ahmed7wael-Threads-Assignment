using OrderProcessing.Classes;
using System.Collections.Generic;

namespace OrderProcessing.Classes;

public class CustomerDashboard
{
    public Customer Customer { get; set; }
    public List<Order> Orders { get; set; } = new List<Order>();
    public List<PaymentTransaction> Payments { get; set; } = new List<PaymentTransaction>();
    public List<EmailMessage> Emails { get; set; } = new List<EmailMessage>();
}