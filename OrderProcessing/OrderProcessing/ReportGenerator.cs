using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OrderProcessing.Classes;
using OrderProcessing.Enums;
using OrderProcessing.Repostries;

namespace OrderProcessing;

public class ReportGenerator
{
    private readonly OrderRepository _orderRepository;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public ReportGenerator(OrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Report> GenerateAsync(CancellationToken ct = default)
    {
        var orders = await _orderRepository.GetAllAsync(ct) ?? new List<Order>();

        List<Product> products = new();
        if (File.Exists(DataFiles.products))
        {
            string productJson = await File.ReadAllTextAsync(DataFiles.products, ct);
            products = JsonSerializer.Deserialize<List<Product>>(productJson, _jsonOptions) ?? new();
        }

        int totalOrders = 0;
        int completedCount = 0;
        int rejectedCount = 0;
        int failedCount = 0;
        int paymentFailedCount = 0;
        decimal totalRevenue = 0;

        foreach (var order in orders)
        {
            totalOrders++;

            if (order.Status == OrderStatus.Completed)
            {
                completedCount++;
                totalRevenue += order.TotalAmount;
            }
            else if (order.Status == OrderStatus.Rejected)
            {
                rejectedCount++;
            }
            else if (order.Status == OrderStatus.Failed)
            {
                failedCount++;
            }
            else if (order.Status == OrderStatus.PaymentFailed)
            {
                paymentFailedCount++;
            }
        }

        decimal averageOrderValue = completedCount > 0 ? totalRevenue / completedCount : 0;

        var productQuantities = new Dictionary<string, int>();

        foreach (var order in orders)
        {
            if (order.Status == OrderStatus.Completed && order.Items != null)
            {
                foreach (var item in order.Items)
                {
                    string prodIdStr = item.ProductId.ToString();
                    if (productQuantities.ContainsKey(prodIdStr))
                    {
                        productQuantities[prodIdStr] += item.Quantity;
                    }
                    else
                    {
                        productQuantities[prodIdStr] = item.Quantity;
                    }
                }
            }
        }

        string topProductId = null;
        int maxQuantity = 0;

        foreach (var kvp in productQuantities)
        {
            if (kvp.Value > maxQuantity)
            {
                maxQuantity = kvp.Value;
                topProductId = kvp.Key;
            }
            else if (kvp.Value == maxQuantity)
            {
                if (topProductId == null || string.Compare(kvp.Key, topProductId, StringComparison.Ordinal) < 0)
                {
                    topProductId = kvp.Key;
                }
            }
        }

        string topProductName = "None";
        if (topProductId != null)
        {
            Product matchedProduct = null;
            foreach (var p in products)
            {
                if (p.Id.ToString() == topProductId)
                {
                    matchedProduct = p;
                    break;
                }
            }
            topProductName = matchedProduct != null ? matchedProduct.Name : $"Product #{topProductId}";
        }

        string topSellingProductResult = $"{topProductName}, {maxQuantity} units";

        var report = new Report
        {
            TotalOrders = totalOrders,
            CompletedCount = completedCount,
            RejectedCount = rejectedCount,
            FailedCount = failedCount,
            PaymentFailedCount = paymentFailedCount,
            TotalRevenue = totalRevenue,
            AverageOrderValue = averageOrderValue,
            TopSellingProduct = topSellingProductResult
        };

        Directory.CreateDirectory(DataFiles.ReportsFolder);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string reportFilePath = Path.Combine(DataFiles.ReportsFolder, $"report_{timestamp}.json");

        string jsonContent = JsonSerializer.Serialize(report, _jsonOptions);
        await File.WriteAllTextAsync(reportFilePath, jsonContent, ct);

        return report;
    }
}