using OrderProcessing;
using OrderProcessing.Classes;
using OrderProcessing.Custom_Exptions;
using OrderProcessing.interfaces;
using OrderProcessing.Repostries;
using OrderProcessing.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// =========================================================================
// === PHASE 1 (Commented Out) =============================================
// =========================================================================

//var jsonFileStore = new JsonFileStore();
//var runner = new Phase1Runner(jsonFileStore);
//await runner.RunAsync();
//Console.WriteLine("\nPress any key to exit...");
//Console.ReadKey();

//var inventoryService = new InventoryService(jsonFileStore);
//int p4ProductId = 4;
//await RaceConditionDemo.RunAsync(inventoryService, jsonFileStore, p4ProductId);


// =========================================================================
// === PHASE 2 (Commented Out) =============================================
// =========================================================================

//var jsonFileStore = new JsonFileStore();
//var inventoryService = new InventoryService(jsonFileStore);
//var productRepository = new ProductRepository(jsonFileStore);
//var customerRepository = new CustomerRepository(jsonFileStore);
//var couponRepository = new CouponRepository(jsonFileStore);
//var orderRepository = new OrderRepository(jsonFileStore);
//var dataSeeder = new DataSeeder(jsonFileStore);
//var pricingService = new PricingService(productRepository, customerRepository, couponRepository, null);
//var invoiceGenerator = new InvoiceGenerator();
//var orderValidator = new OrderValidator(customerRepository, productRepository, couponRepository, orderRepository);
//var orderProcessorV2 = new OrderProcessorV2(inventoryService, pricingService, orderRepository, productRepository, invoiceGenerator);
//var phase2Runner = new Phase2Runner(
//    jsonFileStore,
//    dataSeeder,
//    inventoryService,
//    pricingService,
//    orderProcessorV2,
//    orderValidator,
//    orderRepository
//);
//var cts = new CancellationTokenSource();
//await phase2Runner.RunAsync(cts.Token);
//Console.WriteLine("\nPress any key to exit...");
//Console.ReadKey();


// =========================================================================
// === PHASE 3: Sync vs Async Performance Comparison (Commented Out) =======
// =========================================================================

//Console.WriteLine("\n=== PHASE 3: Performance Benchmark (Sync vs Async) ===");
//var dataStore = new JsonFileStore();
//var dataSeeder = new DataSeeder(dataStore);
//dataSeeder.ResetSync();

//var timeoutIds = new List<int> { 5 };
//var declinedIds = new List<int> { 8 };
//var failAttempts = new Dictionary<int, int> { { 3, 2 }, { 17, 4 } };

//var asyncCustomerRepo = new CustomerRepository(dataStore);
//var asyncProductRepo = new ProductRepository(dataStore);
//var asyncCouponRepo = new CouponRepository(dataStore);
//var asyncOrderRepo = new OrderRepository(dataStore);
//var customerServiceAsync = new CustomerService(asyncCustomerRepo);
//var inventoryServiceAsync = new InventoryService(dataStore);
//var pricingServiceAsync = new PricingService(asyncProductRepo, asyncCustomerRepo, asyncCouponRepo, null);
//var paymentGatewayAsync = new PaymentGateway(dataStore, timeoutIds, declinedIds);
//var emailServiceAsync = new EmailService(dataStore, failAttempts);
//var asyncProcessor = new OrderProcessor(
//    customerServiceAsync,
//    inventoryServiceAsync,
//    pricingServiceAsync,
//    paymentGatewayAsync,
//    asyncOrderRepo,
//    emailServiceAsync
//);

//var syncOrderRepo = new SyncOrderRepository(dataStore);
//var customerServiceSync = new CustomerService(asyncCustomerRepo);
//var inventoryServiceSync = new InventoryService(dataStore);
//var pricingServiceSync = new PricingService(asyncProductRepo, asyncCustomerRepo, asyncCouponRepo, null);
//var paymentGatewaySync = new PaymentGateway(dataStore, timeoutIds, declinedIds);
//var emailServiceSync = new EmailService(dataStore, failAttempts);
//var syncProcessor = new SyncOrderProcessor(
//    customerServiceSync,
//    inventoryServiceSync,
//    pricingServiceSync,
//    paymentGatewaySync,
//    syncOrderRepo,
//    emailServiceSync
//);

//Console.WriteLine("Starting Phase 3 Performance Comparison (Sync vs Async)...");
//await Phase3Runner.RunComparisonAsync(dataSeeder, dataStore, syncProcessor, asyncProcessor);
//await Phase3Runner.VerifyEndStateAsync(dataStore);
//Console.WriteLine("\nPress any key to exit...");
//Console.ReadKey();


// =========================================================================
// === PHASE 4: Scale-Up ===================================================
// =========================================================================

Console.WriteLine("\n=== PHASE 4: Scale-Up Execution ===");
var dataStore = new JsonFileStore();
var dataSeeder = new DataSeeder(dataStore);

var customerRepo = new CustomerRepository(dataStore);
var productRepo = new ProductRepository(dataStore);
var couponRepo = new CouponRepository(dataStore);
var orderRepo = new OrderRepository(dataStore);
var incomingOrderRepo = new IncomingOrderRepository(dataStore);

var customerService = new CustomerService(customerRepo);
var inventoryService = new InventoryService(dataStore);
var pricingService = new PricingService(productRepo, customerRepo, couponRepo, null);

var timeoutIds = new List<int> { 5 };
var declinedIds = new List<int> { 8 };
var failAttempts = new Dictionary<int, int> { { 3, 2 }, { 17, 4 } };

var paymentGateway = new PaymentGateway(dataStore, timeoutIds, declinedIds);
var emailService = new EmailService(dataStore, failAttempts);

var orderProcessor = new OrderProcessor(
    customerService,
    inventoryService,
    pricingService,
    paymentGateway,
    orderRepo,
    emailService
);

// 1. Run Task 10.1 & Automatically triggers Task 10.4 inside Phase4Runner
var phase4Runner = new Phase4Runner(dataSeeder, orderProcessor, incomingOrderRepo, dataStore);
await phase4Runner.RunAsync();

// 2. Run Task 10.2 — Customer Dashboard Execution
Console.WriteLine("\n=== PHASE 4: Customer Dashboard (Task 10.2) ===");
var dashboardService = new CustomerDashboardService(
    customerRepo,
    orderRepo,
    dataStore,
    failPaymentsForCustomerIds: new[] { 99 }
);

Console.WriteLine("\n--- Testing Customer #3 ---");
var sw = Stopwatch.StartNew();
try
{
    var dashboard = await dashboardService.LoadAsync(3);
    sw.Stop();

    var ordersList = new List<string>();
    foreach (var o in dashboard.Orders)
    {
        ordersList.Add($"#{o.Id} {o.Status}");
    }
    string ordersText = string.Join(", ", ordersList);

    var paymentsList = new List<string>();
    foreach (var p in dashboard.Payments)
    {
        paymentsList.Add($"#{p.OrderId} {p.Status}");
    }
    string paymentsText = string.Join(", ", paymentsList);

    var emailsList = new List<string>();
    foreach (var e in dashboard.Emails)
    {
        emailsList.Add($"#{e.OrderId}");
    }
    string emailsText = string.Join(", ", emailsList);

    Console.WriteLine($"{dashboard.Customer.Name} — {dashboard.Orders.Count} orders ({ordersText}), {dashboard.Payments.Count} payments ({paymentsText}), {dashboard.Emails.Count} email ({emailsText})");
    Console.WriteLine($"Total load time: {sw.ElapsedMilliseconds} ms");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("\n--- Testing Customer #99 ---");
try
{
    await dashboardService.LoadAsync(99);
}
catch (AggregateException ex)
{
    var errorMessages = new List<string>();
    foreach (var innerEx in ex.InnerExceptions)
    {
        errorMessages.Add(innerEx.Message);
    }
    Console.WriteLine($"two failures printed: {string.Join(" and ", errorMessages)}");
}
catch (Exception ex)
{
    Console.WriteLine($"Failure printed: {ex.Message}");
}

// 3. Run Task 10.3 — Fastest Shipping Quote Execution
Console.WriteLine("\n=== PHASE 4: Fastest Shipping Quote (Task 10.3) ===");
var carriers = new List<IShippingCarrier>
{
    new FastShip(),
    new CairoExpress(failForOrderIds: new[] { 6 }),
    new NileCourier()
};

var quoteService = new ShippingQuoteService(carriers);
decimal baseShippingCost = 50.00m;

for (int i = 1; i <= 5; i++)
{
    var dummyOrder = new Order { Id = i };
    Console.WriteLine($"\n--- Testing Order #{i} ---");
    try
    {
        var (quote, cancelled) = await quoteService.GetFastestQuoteAsync(dummyOrder, baseShippingCost);
        string cancelledText = cancelled.Count > 0 ? string.Join(", ", cancelled) : "None";
        Console.WriteLine($"Order #{i} -> Winner: {quote.Carrier}, Price: {quote.Price:F2}, Time: {quote.ElapsedMs} ms, Cancelled: [{cancelledText}]");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Order #{i} Failed: {ex.Message}");
    }
}

Console.WriteLine("\n--- Testing Order #6 (All Carriers Should Fail) ---");
try
{
    var failingOrder = new Order { Id = 6 };
    await quoteService.GetFastestQuoteAsync(failingOrder, baseShippingCost);
}
catch (ShippingProviderUnavailableException ex)
{
    Console.WriteLine($"Caught expected exception: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();