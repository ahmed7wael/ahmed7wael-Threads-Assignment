using OrderProcessing.Classes;

namespace OrderProcessing.Repostries;

public class SyncProductRepository
{
    private readonly JsonFileStore _jsonFileStore;
    private const string ClassName = nameof(SyncProductRepository);

    public SyncProductRepository(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
    }
    public Product? GetById(int id, CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        Thread.Sleep(200);

        int quaryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, $"Executing query for product #{id}...");
        Thread.Sleep(quaryDelay);

        var products = _jsonFileStore.Read<Product>(DataFiles.products, ct);
        if (products != null)
        {
            foreach (var product in products)
            {
                if (product.Id == id)
                {
                    Log.Success(ClassName, $"Product #{id} found ({product.Name})");
                    return product;
                }
            }
        }
        Log.Info(ClassName, $"Product #{id} not found");
        return null;
    }

    public bool ReserveStock(int productId, int qty, CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        Thread.Sleep(200);
        Log.Info(ClassName, "Beginning transaction...");
        Thread.Sleep(100);
        Log.Info(ClassName, $"Reserving stock for Product #{productId} (Qty: {qty})...");
        Thread.Sleep(400);

        var products = _jsonFileStore.Read<Product>(DataFiles.products, ct);
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
                    _jsonFileStore.Write<Product>(DataFiles.products, products, ct);

                    Log.Info(ClassName, "Committing transaction...");
                    Thread.Sleep(200);

                    Log.Success(ClassName, $"Successfully reserved {qty} units for Product #{productId}.");
                    return true;
                }
            }
        }
        Log.Error(ClassName, $"Product #{productId} not found.");
        return false;
    }
}
