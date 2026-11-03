using OrderProcessing.Classes;

namespace OrderProcessing;

public class InventoryService
{
    private const string ClassName = nameof(InventoryService);
    private readonly JsonFileStore _jsonFileStore;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public InventoryService(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
    }

    public async Task<bool> ReserveStockUnsafeAsync(int productId, int qty, CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        await Task.Delay(200, ct);

        Log.Info(ClassName, "Beginning transaction...");
        await Task.Delay(100, ct);

        Log.Info(ClassName, $"Reserving stock for Product #{productId} (Qty: {qty})...");
        await Task.Delay(400, ct);

        var products = await _jsonFileStore.ReadAsync<Product>(DataFiles.products, ct);
        if (products != null)
        {
            foreach (var product in products)
            {
                if (product.Id == productId)
                {
                    if (product.StockQuantity < qty || qty < 0)
                    {
                        Log.Error(ClassName, $"Failed to reserve stock for Product #{productId}: Insufficient stock.");
                        return false;
                    }
                    product.StockQuantity -= qty;
                    await _jsonFileStore.WriteAsync<Product>(DataFiles.products, products, ct);

                    Log.Info(ClassName, "Committing transaction...");
                    await Task.Delay(200, ct);

                    Log.Success(ClassName, $"Successfully reserved {qty} units for Product #{productId}.");
                    return true;
                }
            }
        }

        Log.Error(ClassName, $"Product #{productId} not found.");
        return false;
    }

    public async Task<bool> ReserveStockAsync(int productId, int qty, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            Log.Info(ClassName, "Opening connection...");
            await Task.Delay(200, ct);

            Log.Info(ClassName, "Beginning transaction...");
            await Task.Delay(100, ct);

            Log.Info(ClassName, $"Reserving stock safely for Product #{productId} (Qty: {qty})...");
            await Task.Delay(400, ct);

            var products = await _jsonFileStore.ReadAsync<Product>(DataFiles.products, ct);
            if (products != null)
            {
                foreach (var product in products)
                {
                    if (product.Id == productId)
                    {
                        if (product.StockQuantity < qty || qty < 0)
                        {
                            Log.Error(ClassName, $"Failed to reserve stock safely for Product #{productId}: Insufficient stock.");
                            return false;
                        }
                        product.StockQuantity -= qty;
                        await _jsonFileStore.WriteAsync<Product>(DataFiles.products, products, ct);

                        Log.Info(ClassName, "Committing transaction...");
                        await Task.Delay(200, ct);

                        Log.Success(ClassName, $"Successfully reserved {qty} units safely for Product #{productId}.");
                        return true;
                    }
                }
            }

            Log.Error(ClassName, $"Product #{productId} not found.");
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    public async Task ReleaseStockAsync(int productId, int qty, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            Log.Info(ClassName, "Opening connection...");
            await Task.Delay(200, ct);

            Log.Info(ClassName, "Beginning transaction...");
            await Task.Delay(100, ct);

            Log.Info(ClassName, $"Releasing stock for Product #{productId} (Qty: {qty})...");
            await Task.Delay(400, ct);

            var products = await _jsonFileStore.ReadAsync<Product>(DataFiles.products, ct);
            if (products != null)
            {
                foreach (var product in products)
                {
                    if (product.Id == productId)
                    {
                        product.StockQuantity += qty;
                        await _jsonFileStore.WriteAsync<Product>(DataFiles.products, products, ct);

                        Log.Info(ClassName, "Committing transaction...");
                        await Task.Delay(200, ct);

                        Log.Success(ClassName, $"Successfully released {qty} units for Product #{productId}.");
                        return;
                    }
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
