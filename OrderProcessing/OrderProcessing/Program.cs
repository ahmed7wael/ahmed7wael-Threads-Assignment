using System;
using System.Threading;
using System.Threading.Tasks;
using OrderProcessing;
using OrderProcessing.Classes;
using OrderProcessing.Repostries;

var jsonFileStore = new JsonFileStore();

var runner = new Phase1Runner(jsonFileStore);
await runner.RunAsync();

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

var inventoryService = new InventoryService(jsonFileStore);
int p4ProductId = 4;
await RaceConditionDemo.RunAsync(inventoryService, jsonFileStore, p4ProductId);

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