using OrderProcessing.Classes;

namespace OrderProcessing.Services;

public class InventoryService
{
    private const string ClassName = nameof(InventoryService);
    private readonly JsonFileStore _jsonFileStore;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly object _syncLock = new();

    public InventoryService(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
    }

    public async Task<bool> ReserveStockAsync(int productId, int quantity, CancellationToken ct = default)
    {
        var dummyOrder = new Order
        {
            Id = 0,
            Items = [new OrderItem { ProductId = productId, Quantity = quantity }]
        };
        return await ReserveStockAsync(dummyOrder, ct);
    }

    public async Task ReleaseStockAsync(int productId, int quantity, CancellationToken ct = default)
    {
        var dummyOrder = new Order
        {
            Id = 0,
            Items = [new OrderItem { ProductId = productId, Quantity = quantity }]
        };
        await ReleaseStockAsync(dummyOrder, ct);
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

    public async Task<bool> ReserveStockAsync(Order order, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            Log.Info(ClassName, "Opening connection...");
            await Task.Delay(200, ct);

            Log.Info(ClassName, "Beginning transaction...");
            await Task.Delay(100, ct);

            Log.Info(ClassName, $"Reserving stock safely for Order #{order.Id}...");
            await Task.Delay(400, ct);

            var products = await _jsonFileStore.ReadAsync<Product>(DataFiles.products, ct);
            if (products == null)
            {
                Log.Error(ClassName, "Products database is empty.");
                return false;
            }

            foreach (var orderItem in order.Items)
            {
                bool productFound = false;
                foreach (var product in products)
                {
                    if (product.Id == orderItem.ProductId)
                    {
                        productFound = true;
                        if (product.StockQuantity < orderItem.Quantity || orderItem.Quantity < 0)
                        {
                            Log.Error(ClassName, $"Failed to reserve stock: Insufficient stock for Product #{orderItem.ProductId}.");
                            return false;
                        }
                    }
                }

                if (!productFound)
                {
                    Log.Error(ClassName, $"Product #{orderItem.ProductId} not found.");
                    return false;
                }
            }

            foreach (var orderItem in order.Items)
            {
                foreach (var product in products)
                {
                    if (product.Id == orderItem.ProductId)
                    {
                        product.StockQuantity -= orderItem.Quantity;
                    }
                }
            }

            await _jsonFileStore.WriteAsync<Product>(DataFiles.products, products, ct);

            Log.Info(ClassName, "Committing transaction...");
            await Task.Delay(200, ct);

            Log.Success(ClassName, $"Successfully reserved stock for Order #{order.Id}.");
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ReleaseStockAsync(Order order, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            Log.Info(ClassName, "Opening connection...");
            await Task.Delay(200, ct);

            Log.Info(ClassName, "Beginning transaction...");
            await Task.Delay(100, ct);

            Log.Info(ClassName, $"Releasing stock for Order #{order.Id}...");
            await Task.Delay(400, ct);

            var products = await _jsonFileStore.ReadAsync<Product>(DataFiles.products, ct);
            if (products != null)
            {
                foreach (var orderItem in order.Items)
                {
                    foreach (var product in products)
                    {
                        if (product.Id == orderItem.ProductId)
                        {
                            product.StockQuantity += orderItem.Quantity;
                        }
                    }
                }

                await _jsonFileStore.WriteAsync<Product>(DataFiles.products, products, ct);

                Log.Info(ClassName, "Committing transaction...");
                await Task.Delay(200, ct);

                Log.Success(ClassName, $"Successfully released stock for Order #{order.Id}.");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public bool ReserveStockSync(Order order)
    {
        lock (_syncLock)
        {
            Log.Info(ClassName, "Opening connection...");
            Thread.Sleep(200);

            Log.Info(ClassName, "Beginning transaction...");
            Thread.Sleep(100);

            Log.Info(ClassName, $"Reserving stock safely for Order #{order.Id}...");
            Thread.Sleep(400);

            var products = _jsonFileStore.Read<Product>(DataFiles.products);
            if (products == null)
            {
                Log.Error(ClassName, "Products database is empty.");
                return false;
            }

            foreach (var orderItem in order.Items)
            {
                bool productFound = false;
                foreach (var product in products)
                {
                    if (product.Id == orderItem.ProductId)
                    {
                        productFound = true;
                        if (product.StockQuantity < orderItem.Quantity || orderItem.Quantity < 0)
                        {
                            Log.Error(ClassName, $"Failed to reserve stock: Insufficient stock for Product #{orderItem.ProductId}.");
                            return false;
                        }
                    }
                }

                if (!productFound)
                {
                    Log.Error(ClassName, $"Product #{orderItem.ProductId} not found.");
                    return false;
                }
            }

            foreach (var orderItem in order.Items)
            {
                foreach (var product in products)
                {
                    if (product.Id == orderItem.ProductId)
                    {
                        product.StockQuantity -= orderItem.Quantity;
                    }
                }
            }

            _jsonFileStore.Write<Product>(DataFiles.products, products);

            Log.Info(ClassName, "Committing transaction...");
            Thread.Sleep(200);

            Log.Success(ClassName, $"Successfully reserved stock for Order #{order.Id}.");
            return true;
        }
    }

    public void ReleaseStockSync(Order order)
    {
        lock (_syncLock)
        {
            Log.Info(ClassName, "Opening connection...");
            Thread.Sleep(200);

            Log.Info(ClassName, "Beginning transaction...");
            Thread.Sleep(100);

            Log.Info(ClassName, $"Releasing stock for Order #{order.Id}...");
            Thread.Sleep(400);

            var products = _jsonFileStore.Read<Product>(DataFiles.products);
            if (products != null)
            {
                foreach (var orderItem in order.Items)
                {
                    foreach (var product in products)
                    {
                        if (product.Id == orderItem.ProductId)
                        {
                            product.StockQuantity += orderItem.Quantity;
                        }
                    }
                }

                _jsonFileStore.Write<Product>(DataFiles.products, products);

                Log.Info(ClassName, "Committing transaction...");
                Thread.Sleep(200);

                Log.Success(ClassName, $"Successfully released stock for Order #{order.Id}.");
            }
        }
    }
}