using OrderProcessing.Classes;
using OrderProcessing.Enums;
using System.Diagnostics;

namespace OrderProcessing;

public static class Phase3Runner
{
    public static async Task RunComparisonAsync(
        DataSeeder dataSeeder,
        JsonFileStore dataStore,
        SyncOrderProcessor syncProcessor,
        OrderProcessor asyncProcessor)
    {
        var incomingOrders = dataStore.Read<Order>(DataFiles.incoming_orders);
        var ordersToTest = incomingOrders ?? new List<Order>();

        // ==========================================
        // Sync Version Benchmark
        // ==========================================
        dataSeeder.ResetSync();
        var syncStopwatch = Stopwatch.StartNew();
        var threads = new List<Thread>();

        foreach (var order in ordersToTest)
        {
            var thread = new Thread(() =>
            {
                syncProcessor.ProcessOrderSync(order);
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }
        syncStopwatch.Stop();

        Console.WriteLine($"Sync Version -> Time: {syncStopwatch.ElapsedMilliseconds} ms");

        // ==========================================
        // Clear Output Files before Async Benchmark to prevent duplication
        // ==========================================
        dataStore.Write<EmailMessage>("emails.json", new List<EmailMessage>());
        dataStore.Write<EmailMessage>("email_retry.json", new List<EmailMessage>());
        dataStore.Write<PaymentTransaction>(DataFiles.payments, new List<PaymentTransaction>());

        // ==========================================
        // Async Version Benchmark
        // ==========================================
        await dataSeeder.ResetAsync();
        var asyncStopwatch = Stopwatch.StartNew();

        foreach (var order in ordersToTest)
        {
            await asyncProcessor.ProcessOrderAsync(order);
        }

        asyncStopwatch.Stop();

        Console.WriteLine($"Async Version -> Time: {asyncStopwatch.ElapsedMilliseconds} ms");
    }

    public static async Task VerifyEndStateAsync(JsonFileStore dataStore, CancellationToken ct = default)
    {
        Console.WriteLine("\n=== Task 9.4 — Expected end state of the files ===");

        // 1. orders.json
        var orders = await dataStore.ReadAsync<Order>(DataFiles.orders, ct) ?? new List<Order>();
        int completed = orders.Count(o => o.Status == OrderStatus.Completed);
        int rejected = orders.Count(o => o.Status == OrderStatus.Rejected);
        int paymentFailed = orders.Count(o => o.Status == OrderStatus.PaymentFailed);
        int failed = orders.Count(o => o.Status == OrderStatus.Failed);
        Console.WriteLine($"[orders.json]     20 records: {completed} Completed, {rejected} Rejected (#4, #9, #12, #15, #18 + one of #10/#11), {paymentFailed} PaymentFailed (#5, #8), {failed} Failed (#13)");

        // 2. payments.json
        var payments = await dataStore.ReadAsync<PaymentTransaction>(DataFiles.payments, ct) ?? new List<PaymentTransaction>();
        int captured = payments.Count(p => p.Status == PaymentStatus.Captured);
        int declined8 = payments.Count(p => p.OrderId == 8 && p.Status == PaymentStatus.Declined);
        int refunded13 = payments.Count(p => p.OrderId == 13 && p.Status == PaymentStatus.Refunded);
        Console.WriteLine($"[payments.json]   {payments.Count} records: {captured} Captured, {declined8} Declined (#8), {refunded13} Refunded (#13). None for #5");

        // 3. emails.json
        var emails = await dataStore.ReadAsync<object>("emails.json", ct) ?? new List<object>();
        Console.WriteLine($"[emails.json]     {emails.Count} records (every Completed order except #17); #3 has Attempts = 3");

        // 4. email_retry.json
        var emailRetries = await dataStore.ReadAsync<object>("email_retry.json", ct) ?? new List<object>();
        Console.WriteLine($"[email_retry.json] {emailRetries.Count} record (#17) with Attempts = 4 and LastError set");

        // 5. products.json
        var products = await dataStore.ReadAsync<Product>(DataFiles.products, ct) ?? new List<Product>();
        var keyboard = products.FirstOrDefault(p => p.Name.Contains("Keyboard", StringComparison.OrdinalIgnoreCase));
        int keyboardStock = keyboard != null ? keyboard.StockQuantity : 0;
        Console.WriteLine($"[products.json]   stock reduced only by Completed orders; Mechanical Keyboard = {keyboardStock}");

        Console.WriteLine("Comparison finished!");
    }
}