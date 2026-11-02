using System.Diagnostics;
using OrderProcessing.Classes;
using OrderProcessing.Enums;
using OrderProcessing.Repostries;

namespace OrderProcessing;

public class Phase1Runner
{
    private readonly JsonFileStore _jsonFileStore;
    private readonly DataSeeder _dataSeeder;
    private const string ClassName = nameof(Phase1Runner);

    private readonly SyncCustomerRepository _syncCustomerRepo;
    private readonly SyncProductRepository _syncProductRepo;
    private readonly SyncCouponRepository _syncCouponRepo;
    private readonly SyncOrderValidator _syncOrderValidator;

    private readonly CustomerRepository _customerRepo;
    private readonly ProductRepository _productRepo;
    private readonly CouponRepository _couponRepo;
    private readonly OrderRepository _orderRepo;
    private readonly OrderValidator _orderValidator;

    public Phase1Runner(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
        _dataSeeder = new DataSeeder(_jsonFileStore);

        // Sync dependencies (Results not saved in sync run, so order repo is not needed here)
        _syncCustomerRepo = new SyncCustomerRepository(_jsonFileStore);
        _syncProductRepo = new SyncProductRepository(_jsonFileStore);
        _syncCouponRepo = new SyncCouponRepository(_jsonFileStore);
        _syncOrderValidator = new SyncOrderValidator(_syncCustomerRepo, _syncProductRepo, _syncCouponRepo);

        // Async dependencies
        _customerRepo = new CustomerRepository(_jsonFileStore); 
        _productRepo = new ProductRepository(_jsonFileStore);
        _couponRepo = new CouponRepository(_jsonFileStore);
        _orderRepo = new OrderRepository(_jsonFileStore);
        _orderValidator = new OrderValidator(_customerRepo, _productRepo, _couponRepo, _orderRepo);
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        Log.Info(ClassName, "=== Starting Task 7.3 Benchmark (Sync vs Async, Sequential) ===");

        // ==========================================
        // 1. SYNC RUN
        // ==========================================
        Log.Info(ClassName, "--- Preparing for Sync Run ---");
        await _dataSeeder.ResetAsync(ct);

        var incomingOrdersSync = _jsonFileStore.Read<Order>(DataFiles.incoming_orders, ct) ?? new List<Order>();
        var swSync = Stopwatch.StartNew();
        foreach (var order in incomingOrdersSync)
        {
            _syncOrderValidator.ValidateOrder(order, ct);
        }
        swSync.Stop();
        var timeSync = swSync.ElapsedMilliseconds;
        Log.Success(ClassName, $"Sync Run completed in {timeSync} ms.");

        // ==========================================
        // 2. ASYNC RUN
        // ==========================================
        Log.Info(ClassName, "--- Preparing for Async Run ---"); // تم تصحيح اللوج هنا
        await _dataSeeder.ResetAsync(ct);

        var incomingOrders = await _jsonFileStore.ReadAsync<Order>(DataFiles.incoming_orders, ct) ?? new List<Order>();
        var swAsync = Stopwatch.StartNew();
        foreach (var order in incomingOrders)
        {
            await _orderValidator.ValidateOrderAsync(order, ct); // تمرير الـ ct هنا أيضاً
        }
        swAsync.Stop();
        var timeAsync = swAsync.ElapsedMilliseconds;
        Log.Success(ClassName, $"Async Run completed in {timeAsync} ms.");

        // ==========================================
        // 3. SUMMARY REPORT (R3)
        // ==========================================
        var allOrders = await _orderRepo.GetAllAsync(ct);
        var validatedOrders = await _orderRepo.GetByStatusAsync(OrderStatus.Validated, ct);
        var rejectedOrders = await _orderRepo.GetByStatusAsync(OrderStatus.Rejected, ct);

        Console.WriteLine("\n===== Validation Summary =====");
        Console.WriteLine($"Total Orders: {incomingOrders.Count}");
        Console.WriteLine($"Validated: {validatedOrders.Count}");
        Console.WriteLine($"Rejected: {rejectedOrders.Count}");

        foreach (var rejected in rejectedOrders)
        {
            Console.WriteLine($"    - Order #{rejected.Id}: {rejected.FailureReason}");
        }

        double syncSeconds = timeSync / 1000.0;
        double asyncSeconds = timeAsync / 1000.0;

        Console.WriteLine($"Sync Time:  {syncSeconds:F1} seconds");
        Console.WriteLine($"Async Time: {asyncSeconds:F1} seconds");
        Console.WriteLine($"orders.json now contains {allOrders.Count} records.\n");
    }
}