using OrderProcessing.Classes;
using OrderProcessing.Services;

namespace OrderProcessing;

public static class RaceConditionDemo
{
    public static async Task RunAsync(InventoryService inventoryService, JsonFileStore jsonFileStore, int p4ProductId, CancellationToken ct = default)
    {
        Console.WriteLine("=== Task 8.3: Race Condition on the Last Keyboard ===");

        Console.WriteLine("\n--- 1. Unsafe Reservation Demo ---");

        var products = await jsonFileStore.ReadAsync<Product>(DataFiles.products, ct);
        if (products != null)
        {
            foreach (var p in products)
            {
                if (p.Id == p4ProductId)
                {
                    p.StockQuantity = 1;
                    break;
                }
            }
            await jsonFileStore.WriteAsync<Product>(DataFiles.products, products, ct);
        }

        var task10Unsafe = inventoryService.ReserveStockUnsafeAsync(p4ProductId, 1, ct);
        var task11Unsafe = inventoryService.ReserveStockUnsafeAsync(p4ProductId, 1, ct);

        bool result10Unsafe = await task10Unsafe;
        bool result11Unsafe = await task11Unsafe;

        products = await jsonFileStore.ReadAsync<Product>(DataFiles.products, ct);
        int finalStockUnsafe = -1;
        if (products != null)
        {
            foreach (var p in products)
            {
                if (p.Id == p4ProductId)
                {
                    finalStockUnsafe = p.StockQuantity;
                    break;
                }
            }
        }

        Console.WriteLine($"Order #10 Unsafe Result: {result10Unsafe}");
        Console.WriteLine($"Order #11 Unsafe Result: {result11Unsafe}");
        Console.WriteLine($"Stock read back from file (Unsafe): {finalStockUnsafe}");

        Console.WriteLine("\n--- 2. Safe Reservation Demo (Fixed) ---");

        products = await jsonFileStore.ReadAsync<Product>(DataFiles.products, ct);
        if (products != null)
        {
            foreach (var p in products)
            {
                if (p.Id == p4ProductId)
                {
                    p.StockQuantity = 1;
                    break;
                }
            }
            await jsonFileStore.WriteAsync<Product>(DataFiles.products, products, ct);
        }

        var task10Safe = inventoryService.ReserveStockAsync(p4ProductId, 1, ct);
        var task11Safe = inventoryService.ReserveStockAsync(p4ProductId, 1, ct);

        bool result10Safe = await task10Safe;
        bool result11Safe = await task11Safe;

        products = await jsonFileStore.ReadAsync<Product>(DataFiles.products, ct);
        int finalStockSafe = -1;
        if (products != null)
        {
            foreach (var p in products)
            {
                if (p.Id == p4ProductId)
                {
                    finalStockSafe = p.StockQuantity;
                    break;
                }
            }
        }

        Console.WriteLine($"Order #10 Safe Result: {result10Safe}");
        Console.WriteLine($"Order #11 Safe Result: {result11Safe}");
        Console.WriteLine($"Stock read back from file (Safe): {finalStockSafe}");
    }
}
