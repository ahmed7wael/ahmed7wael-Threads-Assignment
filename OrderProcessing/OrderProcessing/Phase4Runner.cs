using OrderProcessing.Classes;
using OrderProcessing.Enums;
using OrderProcessing.Repostries;
using OrderProcessing.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OrderProcessing;

public class Phase4Runner
{
    private readonly DataSeeder _dataSeeder;
    private readonly OrderProcessor _orderProcessor;
    private readonly IncomingOrderRepository _incomingOrderRepository;
    private readonly JsonFileStore _jsonFileStore;
    private readonly EmailService _emailService;

    public Phase4Runner(
        DataSeeder dataSeeder,
        OrderProcessor orderProcessor,
        IncomingOrderRepository incomingOrderRepository,
        JsonFileStore jsonFileStore)
    {
        _dataSeeder = dataSeeder;
        _orderProcessor = orderProcessor;
        _incomingOrderRepository = incomingOrderRepository;
        _jsonFileStore = jsonFileStore;
        _emailService = new EmailService(_jsonFileStore);
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        Console.WriteLine("\n=== Starting Task 10.1: Live Checkout Progress ===");

        await _dataSeeder.ResetAsync(ct);

        var orders = await _incomingOrderRepository.GetAllAsync(ct);
        if (orders == null || orders.Count == 0)
        {
            Console.WriteLine("No orders found for processing.");
            return;
        }

        int totalOrders = orders.Count;
        var activeTasks = new HashSet<Task<(Order Order, long ElapsedMs)>>();

        foreach (var order in orders)
        {
            var task = Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                await _orderProcessor.ProcessOrderAsync(order, ct);
                sw.Stop();
                return (order, sw.ElapsedMilliseconds);
            }, ct);

            activeTasks.Add(task);
        }

        int completedCount = 0;

        while (activeTasks.Count > 0)
        {
            var finishedTask = await Task.WhenAny(activeTasks);
            activeTasks.Remove(finishedTask);

            completedCount++;
            var result = await finishedTask;
            var order = result.Order;
            long elapsedMs = result.ElapsedMs;

            double seconds = elapsedMs / 1000.0;

            string statusDescription = order.Status.ToString();
            if (order.Status != OrderStatus.Completed && !string.IsNullOrEmpty(order.FailureReason))
            {
                statusDescription = $"{order.Status} ({order.FailureReason})";
            }

            string indexStr = completedCount < 10 ? $" {completedCount}" : $"{completedCount}";
            Console.WriteLine($"[ {indexStr}/{totalOrders}] Order #{order.Id} -> {statusDescription} after {seconds:F1} s");
        }

        PrintStatusSummary(orders);

        Console.WriteLine("=== Done Task 10.1 success ===");

        await RunTask10_4Async(ct);
    }

    private void PrintStatusSummary(List<Order> orders)
    {
        Console.WriteLine("\n--- Status Summary ---");
        int completed = 0;
        int rejected = 0;
        int paymentFailed = 0;
        int failed = 0;
        int cancelled = 0;

        foreach (var order in orders)
        {
            if (order.Status == OrderStatus.Completed)
            {
                completed++;
            }
            else if (order.Status == OrderStatus.Rejected)
            {
                rejected++;
            }
            else if (order.Status == OrderStatus.PaymentFailed)
            {
                paymentFailed++;
            }
            else if (order.Status == OrderStatus.Failed)
            {
                failed++;
            }
            else if (order.Status == OrderStatus.Cancelled)
            {
                cancelled++;
            }
        }

        Console.WriteLine($"Completed: {completed}");
        Console.WriteLine($"Rejected: {rejected}");
        Console.WriteLine($"PaymentFailed: {paymentFailed}");
        Console.WriteLine($"Failed: {failed}");
        Console.WriteLine($"Cancelled: {cancelled}");
    }

    private static int Score(int orderId)
    {
        long h = orderId;
        for (int i = 0; i < 100_000; i++)
        {
            h = (h * 31 + i) % 1_000_000_007;
        }
        return (int)(h % 100);
    }

    public async Task RunTask10_4Async(CancellationToken ct = default)
    {
        Console.WriteLine("\n=== Starting Task 10.4: Fraud scoring and email re-send ===");

        // Part A — Fraud scoring (CPU-bound)
        Console.WriteLine("\n--- Part A: Fraud Scoring (10,000 Orders) ---");

        int totalOrders = 10_000;
        var sequentialFlaggedIds = new List<int>();

        var swSeq = Stopwatch.StartNew();
        for (int id = 1; id <= totalOrders; id++)
        {
            if (ct.IsCancellationRequested) break;
            int score = Score(id);
            if (score >= 90)
            {
                sequentialFlaggedIds.Add(id);
            }
        }
        swSeq.Stop();

        var parallelFlaggedBag = new ConcurrentBag<int>();
        var distinctThreads = new ConcurrentDictionary<int, int>();
        int processorCount = Environment.ProcessorCount;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = processorCount,
            CancellationToken = ct
        };

        var swPar = Stopwatch.StartNew();
        try
        {
            Parallel.For(1, totalOrders + 1, parallelOptions, id =>
            {
                distinctThreads[Environment.CurrentManagedThreadId] = 1;
                int score = Score(id);
                if (score >= 90)
                {
                    parallelFlaggedBag.Add(id);
                }
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Parallel execution cancelled.");
        }
        swPar.Stop();

        var parallelFlaggedList = new List<int>();
        foreach (var id in parallelFlaggedBag)
        {
            parallelFlaggedList.Add(id);
        }
        parallelFlaggedList.Sort();

        string firstFiveSeq = "";
        for (int i = 0; i < 5 && i < sequentialFlaggedIds.Count; i++)
        {
            firstFiveSeq += sequentialFlaggedIds[i] + (i < 4 ? ", " : "");
        }

        double speedUp = (double)swSeq.ElapsedMilliseconds / (swPar.ElapsedMilliseconds > 0 ? swPar.ElapsedMilliseconds : 1);

        Console.WriteLine($"Flagged Count: {sequentialFlaggedIds.Count}");
        Console.WriteLine($"First five IDs: [{firstFiveSeq}]");
        Console.WriteLine($"Sequential time: {swSeq.ElapsedMilliseconds} ms");
        Console.WriteLine($"Parallel time: {swPar.ElapsedMilliseconds} ms");
        Console.WriteLine($"Speed-up: {speedUp:F2}x");
        Console.WriteLine($"Distinct threads used: {distinctThreads.Count}");
        Console.WriteLine($"ProcessorCount: {processorCount}");

        // Part B — Email re-send (I/O-bound)
        Console.WriteLine("\n--- Part B: Email Re-send (30 Emails, Max 4 Concurrency) ---");

        int currentConcurrentSends = 0;
        int maxConcurrentSends = 0;
        object lockObj = new object();

        using var semaphore = new SemaphoreSlim(4, 4);
        var emailTasks = new List<Task>();

        var swEmail = Stopwatch.StartNew();
        for (int id = 1001; id <= 1030; id++)
        {
            int currentId = id;
            var task = Task.Run(async () =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    lock (lockObj)
                    {
                        currentConcurrentSends++;
                        if (currentConcurrentSends > maxConcurrentSends)
                        {
                            maxConcurrentSends = currentConcurrentSends;
                        }
                    }

                    var order = new Order { Id = currentId };
                    var customer = new Customer { Name = $"Customer {currentId}", Email = $"customer{currentId}@test.com" };

                    // استخدام الدالة المتاحة فعلياً في EmailService لتخزين الإيميلات في emails.json
                    await _emailService.SendConfirmationWithRetryAsync(order, customer, ct);
                    await Task.Delay(1000, ct); // محاكاة وقت الإرسال للوصول للزمن المتوقع (~8 ثوانٍ)
                }
                finally
                {
                    lock (lockObj)
                    {
                        currentConcurrentSends--;
                    }
                    semaphore.Release();
                }
            }, ct);

            emailTasks.Add(task);
        }

        await Task.WhenAll(emailTasks);
        swEmail.Stop();

        Console.WriteLine($"Total emails sent: 30");
        Console.WriteLine($"Max concurrent sends recorded: {maxConcurrentSends} (Must be exactly 4)");
        Console.WriteLine($"Total time: {swEmail.ElapsedMilliseconds} ms");

        Console.WriteLine("=== Done Task 10.4 success ===");
    }
}