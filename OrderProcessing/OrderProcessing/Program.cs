using OrderProcessing;
using OrderProcessing.Classes;
using OrderProcessing.interfaces;
using OrderProcessing.Repostries;
using OrderProcessing.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

// =========================================================================
// === PHASE 1 (Commented Out) =============================================
// =========================================================================
/*
var jsonFileStore = new JsonFileStore();
var runner = new Phase1Runner(jsonFileStore);
await runner.RunAsync();
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

var inventoryService = new InventoryService(jsonFileStore);
int p4ProductId = 4;
await RaceConditionDemo.RunAsync(inventoryService, jsonFileStore, p4ProductId);
*/

// =========================================================================
// === PHASE 2 (Commented Out) =============================================
// =========================================================================
/*
var jsonFileStore = new JsonFileStore();
var inventoryService = new InventoryService(jsonFileStore);
var productRepository = new ProductRepository(jsonFileStore);
var customerRepository = new CustomerRepository(jsonFileStore);
var couponRepository = new CouponRepository(jsonFileStore);
var orderRepository = new OrderRepository(jsonFileStore);
var dataSeeder = new DataSeeder(jsonFileStore);
var pricingService = new PricingService(productRepository, customerRepository, couponRepository, null);
var invoiceGenerator = new InvoiceGenerator();
var orderValidator = new OrderValidator(customerRepository, productRepository, couponRepository, orderRepository);
var orderProcessorV2 = new OrderProcessorV2(inventoryService, pricingService, orderRepository, productRepository, invoiceGenerator);
var phase2Runner = new Phase2Runner(
    jsonFileStore,
    dataSeeder,
    inventoryService,
    pricingService,
    orderProcessorV2,
    orderValidator,
    orderRepository
);
var cts = new CancellationTokenSource();
await phase2Runner.RunAsync(cts.Token);
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
*/

// =========================================================================
// === PHASE 3: Sync vs Async Performance Comparison (Active) ==============
// =========================================================================
Console.WriteLine("\n=== PHASE 3: Performance Benchmark (Sync vs Async) ===");

var dataStore = new JsonFileStore();
var dataSeeder = new DataSeeder(dataStore);
dataSeeder.ResetSync();

// تمرير الـ IDs الصحيحة مطابقة للجدول
var timeoutIds = new List<int> { 5 };       // الأوردر رقم 5 يحصل له Timeout
var declinedIds = new List<int> { 8 };      // الأوردر رقم 8 يحصل له Decline
var failAttempts = new Dictionary<int, int> { { 3, 2 }, { 17, 4 } }; // #3 يفشل مرتين وينجح الثالثة، #17 يفشل 4 مرات ويدخل الـ retry

// --- 1. Async Repositories & Services ---
var asyncCustomerRepo = new CustomerRepository(dataStore);
var asyncProductRepo = new ProductRepository(dataStore);
var asyncCouponRepo = new CouponRepository(dataStore);
var asyncOrderRepo = new OrderRepository(dataStore);
var customerServiceAsync = new CustomerService(asyncCustomerRepo);
var inventoryServiceAsync = new InventoryService(dataStore);
var pricingServiceAsync = new PricingService(asyncProductRepo, asyncCustomerRepo, asyncCouponRepo, null);

var paymentGatewayAsync = new PaymentGateway(dataStore, timeoutIds, declinedIds);
var emailServiceAsync = new EmailService(dataStore, failAttempts);
var asyncProcessor = new OrderProcessor(
    customerServiceAsync,
    inventoryServiceAsync,
    pricingServiceAsync,
    paymentGatewayAsync,
    asyncOrderRepo,
    emailServiceAsync
);

// --- 2. Sync Repositories & Services ---
var syncOrderRepo = new SyncOrderRepository(dataStore);
var customerServiceSync = new CustomerService(asyncCustomerRepo);
var inventoryServiceSync = new InventoryService(dataStore);
var pricingServiceSync = new PricingService(asyncProductRepo, asyncCustomerRepo, asyncCouponRepo, null);

var paymentGatewaySync = new PaymentGateway(dataStore, timeoutIds, declinedIds);
var emailServiceSync = new EmailService(dataStore, failAttempts);
var syncProcessor = new SyncOrderProcessor(
    customerServiceSync,
    inventoryServiceSync,
    pricingServiceSync,
    paymentGatewaySync,
    syncOrderRepo,
    emailServiceSync
);

Console.WriteLine("Starting Phase 3 Performance Comparison (Sync vs Async)...");

// Run Benchmark & Verification
await Phase3Runner.RunComparisonAsync(dataSeeder, dataStore, syncProcessor, asyncProcessor);
await Phase3Runner.VerifyEndStateAsync(dataStore);

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
Console.WriteLine("\n--- Testing Task 9.7 ---");
var sampleOrder = new Order { Id = 99, CustomerId = 1 };
var sampleCustomer = new Customer { Id = 1, Name = "Test Customer", Email = "test@mail.com", IsActive = true }; ;
await asyncProcessor.ProcessOrderAsync(sampleOrder, sampleCustomer);
Console.WriteLine("\n--- solved ---");
var sampleOrder1 = new Order { Id = 99, CustomerId = 1 };
var sampleCustomer1 = new Customer { Id = 1, Name = "Test Customer", Email = "test@mail.com", IsActive = true }; ;
await asyncProcessor.ProcessOrderAsync(sampleOrder1, sampleCustomer1);