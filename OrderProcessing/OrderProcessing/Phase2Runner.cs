using OrderProcessing.Classes;
using OrderProcessing.Enums;
using OrderProcessing.Repostries;
using OrderProcessing.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OrderProcessing;

public class Phase2Runner
{
    private readonly JsonFileStore _jsonFileStore;
    private readonly DataSeeder _dataSeeder;
    private readonly InventoryService _inventoryService;
    private readonly PricingService _pricingService;
    private readonly OrderProcessorV2 _orderProcessorV2;
    private readonly OrderValidator _orderValidator;
    private readonly OrderRepository _orderRepository;

    public Phase2Runner(JsonFileStore jsonFileStore, DataSeeder dataSeeder, InventoryService inventoryService, PricingService pricingService, OrderProcessorV2 orderProcessorV2, OrderValidator orderValidator, OrderRepository orderRepository)
    {
        _jsonFileStore = jsonFileStore;
        _dataSeeder = dataSeeder;
        _inventoryService = inventoryService;
        _pricingService = pricingService;
        _orderProcessorV2 = orderProcessorV2;
        _orderValidator = orderValidator;
        _orderRepository = orderRepository;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        await _dataSeeder.ResetAsync(ct);
        await RaceConditionDemo.RunAsync(_inventoryService, _jsonFileStore, 4, ct);
        await _dataSeeder.ResetAsync(ct);

        var incomingOrders = await _jsonFileStore.ReadAsync<Order>(DataFiles.incoming_orders, ct);
        if (incomingOrders != null && incomingOrders.Count > 0)
        {
            var order1 = incomingOrders[0];
            var asyncResult = await _pricingService.PriceOrderAsync(order1, ct);
            var seqResult = await _pricingService.PriceOrderSequentialAsync(order1, ct);

            Console.WriteLine($"\n--- Pricing Comparison for Order #{order1.Id} ---");
            Console.WriteLine($"Async Price: {asyncResult.totalAmount} (Elapsed: {asyncResult.elapsedMs}ms)");
            Console.WriteLine($"Sequential Price: {seqResult.totalAmount} (Elapsed: {seqResult.elapsedMs}ms)");
        }

        if (incomingOrders != null)
        {
            var validationTasks = new List<Task<bool>>();
            foreach (var order in incomingOrders)
            {
                var task = _orderValidator.ValidateOrderAsync(order, ct);
                validationTasks.Add(task);
            }
            foreach (var task in validationTasks)
            {
                await task;
            }
        }

        var processingTasks = new List<Task<bool>>();
        var validatedOrders = new List<Order>();
        var taskOrderMap = new Dictionary<Task<bool>, Order>();

        if (incomingOrders != null)
        {
            foreach (var order in incomingOrders)
            {
                if (order.Status == OrderStatus.Validated)
                {
                    validatedOrders.Add(order);
                }
            }

            foreach (var order in validatedOrders)
            {
                var task = _orderProcessorV2.ProcessValidatedOrderAsync(order, ct);
                processingTasks.Add(task);
                taskOrderMap[task] = order;
            }

            if (processingTasks.Count > 0)
            {
                Console.WriteLine($"Status of task[0] immediately after creation: {processingTasks[0].Status}");
            }

            await Task.Delay(1000, ct);
            foreach (var task in processingTasks)
            {
                Console.WriteLine($"Task Status (while running): {task.Status}");
            }

            Console.WriteLine("\n=== Step 5: Awaiting tasks and handling any failures safely ===");
            foreach (var task in processingTasks)
            {
                var order = taskOrderMap[task];
                try
                {
                    bool success = await task;
                    if (!success)
                    {
                        Log.Error(nameof(Phase2Runner), $"Task Status: {task.Status} | Order #{order.Id} failed processing.");
                        order.Status = OrderStatus.Failed;
                        order.FailureReason = "Processing returned false";

                        if (order.Items != null)
                        {
                            foreach (var item in order.Items)
                            {
                                await _inventoryService.ReleaseStockAsync(item.ProductId, item.Quantity, ct);
                            }
                        }

                        await _orderRepository.SaveAsync(order, ct);
                    }
                    else
                    {
                        Log.Success(nameof(Phase2Runner), $"Task Status: {task.Status} | Order #{order.Id} processed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(Phase2Runner), $"Task Status: {task.Status} | Order #{order.Id} exception: {ex.Message}");
                    order.Status = OrderStatus.Failed;
                    order.FailureReason = ex.Message;

                    if (order.Items != null)
                    {
                        foreach (var item in order.Items)
                        {
                            await _inventoryService.ReleaseStockAsync(item.ProductId, item.Quantity, ct);
                        }
                    }

                    await _orderRepository.SaveAsync(order, ct);
                }
            }
        }

        stopwatch.Stop();

        Console.WriteLine("\n=== Step 6: Final Task Statuses and Total Time ===");
        foreach (var task in processingTasks)
        {
            Console.WriteLine($"Final Task Status: {task.Status}");
        }
        Console.WriteLine($"Total execution time: {stopwatch.ElapsedMilliseconds}ms");

        Console.WriteLine("\n=== Step 7: Confirm orders.json records ===");
        var finalSavedOrders = await _jsonFileStore.ReadAsync<Order>(DataFiles.orders, ct);
        int finalCount = finalSavedOrders != null ? finalSavedOrders.Count : 0;
        Console.WriteLine($"Total records found in orders.json: {finalCount}");

        if (finalCount == 20)
        {
            Log.Success(nameof(Phase2Runner), "SUCCESS: orders.json contains exactly 20 records. No lost writes!");
        }
        else
        {
            Log.Error(nameof(Phase2Runner), $"WARNING: Expected 20 records, but found {finalCount}.");
        }
    }
}