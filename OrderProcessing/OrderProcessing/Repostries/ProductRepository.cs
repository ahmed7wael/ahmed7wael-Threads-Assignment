using OrderProcessing.Classes;

namespace OrderProcessing.Repostries;

public class ProductRepository
{
    private readonly JsonFileStore _jsonFileStore;
    private const string ClassName = nameof(ProductRepository);

    public ProductRepository(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
    }
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        await Task.Delay(200, ct);

        int quaryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, $"Executing query for product #{id}...");
        await Task.Delay(quaryDelay, ct);

        var products = await _jsonFileStore.ReadAsync<Product>(DataFiles.products, ct); 
        if(products != null)
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

    public async Task<bool> ReserveStockAsync(int productId, int qty , CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        await Task.Delay(200, ct);
        Log.Info(ClassName, "Beginning transaction...");
        await Task.Delay(100, ct);
        Log.Info(ClassName, $"Reserving stock for Product #{productId} (Qty: {qty})...");   
        await Task.Delay(400, ct);

        var products = await _jsonFileStore.ReadAsync<Product>(DataFiles.products, ct);
        if(products != null)
        {
            foreach(var product in products)
            {
                if(product.Id == productId)
                {
                    if(product.StockQuantity < qty || qty < 0)
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
 }
