using OrderProcessing.Classes;

namespace OrderProcessing.Repostries;

public class IncomingOrderRepository
{
    private readonly JsonFileStore _jsonFileStore;
    private const string ClassName = nameof(IncomingOrderRepository);

    public IncomingOrderRepository(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
    }
    public async Task<List<Order>> GetAllAsync(CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        await Task.Delay(200, ct);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, "Executing query to get all orders...");
        await Task.Delay(queryDelay, ct);

        var orders = await _jsonFileStore.ReadAsync<Order>(DataFiles.incoming_orders, ct);
        return orders ?? new List<Order>();
    }
}
