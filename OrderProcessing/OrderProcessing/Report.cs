namespace OrderProcessing;

public class Report
{
    public int TotalOrders { get; set; }
    public int CompletedCount { get; set; }
    public int RejectedCount { get; set; }
    public int FailedCount { get; set; }
    public int PaymentFailedCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public string TopSellingProduct { get; set; } = string.Empty;
}
