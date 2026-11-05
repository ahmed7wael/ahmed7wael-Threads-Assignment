using OrderProcessing.Classes;
using OrderProcessing.Custom_Exptions;
using OrderProcessing.CustomExceptions;
using OrderProcessing.interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OrderProcessing.Services;

public class ShippingQuoteService
{
    private readonly List<IShippingCarrier> _carriers;

    public ShippingQuoteService(List<IShippingCarrier> carriers)
    {
        _carriers = carriers;
    }

    public async Task<(ShippingQuote Quote, List<string> CancelledCarriers)> GetFastestQuoteAsync(Order order, decimal baseShippingCost, CancellationToken ct = default)
    {
        using var budgetCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1200));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, budgetCts.Token);

        var taskToCarrier = new Dictionary<Task<decimal>, IShippingCarrier>();
        var runningTasks = new List<Task<decimal>>();

        foreach (var carrier in _carriers)
        {
            var task = Task.Run(async () =>
            {
                return await carrier.GetQuoteAsync(order, baseShippingCost, linkedCts.Token);
            }, linkedCts.Token);

            taskToCarrier[task] = carrier;
            runningTasks.Add(task);
        }

        var cancelledCarriers = new List<string>();
        var swTotal = Stopwatch.StartNew();
        int failedCount = 0;

        while (runningTasks.Count > 0)
        {
            Task<decimal> finishedTask;
            try
            {
                finishedTask = await Task.WhenAny(runningTasks);
                runningTasks.Remove(finishedTask);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var carrier = taskToCarrier[finishedTask];

            try
            {
                decimal price = await finishedTask;
                swTotal.Stop();

                var winningQuote = new ShippingQuote
                {
                    Carrier = carrier.Name,
                    Price = price,
                    ElapsedMs = swTotal.ElapsedMilliseconds,
                    IsSuccessful = true
                };

                budgetCts.Cancel();

                foreach (var remainingTask in runningTasks)
                {
                    var remainingCarrier = taskToCarrier[remainingTask];
                    cancelledCarriers.Add(remainingCarrier.Name);
                    Console.WriteLine($"[{remainingCarrier.Name}] cancelled");
                }

                return (winningQuote, cancelledCarriers);
            }
            catch (Exception)
            {
                failedCount++;
            }
        }

        swTotal.Stop();

        if (failedCount >= _carriers.Count)
        {
            throw new ShippingProviderUnavailableException("All carriers failed");
        }

        var flatRateQuote = new ShippingQuote
        {
            Carrier = "Flat rate",
            Price = 25.00m,
            ElapsedMs = 1200,
            IsSuccessful = false
        };

        foreach (var carrier in _carriers)
        {
            cancelledCarriers.Add(carrier.Name);
            Console.WriteLine($"[{carrier.Name}] cancelled");
        }

        return (flatRateQuote, cancelledCarriers);
    }
}
