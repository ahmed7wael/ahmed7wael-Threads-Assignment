using OrderProcessing.Classes;
using OrderProcessing.Enums;

namespace OrderProcessing.Repostries;

public class OrderRepository
{
    private readonly JsonFileStore _jsonFileStore;
    private const string ClassName = nameof(OrderRepository);

    public OrderRepository(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
    }
    public async Task SaveAsync(Order order , CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        await Task.Delay(200, ct);

        Log.Info(ClassName, "Beginning transaction...");
        await Task.Delay(100, ct);

        Log.Info(ClassName, $"Saving new Order #{order.Id}...");
        await Task.Delay(400, ct);

        var orders = await _jsonFileStore.ReadAsync<Order>(DataFiles.orders, ct);
        if(orders == null)
        {
            orders = new List<Order>();
        }
        bool found = false;
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].Id == order.Id)
            {
                orders[i] = order; 
                found = true;
                break;
            }
        }

        if (!found)
        {
            orders.Add(order);
        }
        await _jsonFileStore.WriteAsync(DataFiles.orders, orders, ct);

        Log.Info(ClassName, "Committing transaction...");
        await Task.Delay(200, ct);

        Log.Success(ClassName, $"Order #{order.Id} successfully saved.");

    }

    public async Task<List<Order>> GetAllAsync(CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        await Task.Delay(200, ct);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, "Executing query to get all orders...");
        await Task.Delay(queryDelay, ct);

        var orders = await _jsonFileStore.ReadAsync<Order>(DataFiles.orders, ct);
        return orders ?? new List<Order>();
    }
    public async Task<List<Order>> GetByStatusAsync(OrderStatus s, CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        await Task.Delay(200, ct);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, $"Executing query for orders with status '{s}'...");
        await Task.Delay(queryDelay, ct);

        var allOrders = await _jsonFileStore.ReadAsync<Order>(DataFiles.orders, ct);
        var filteredOrders = new List<Order>();

        if (allOrders != null)
        {
            foreach (var order in allOrders)
            {
                if (order.Status == s)
                {
                    filteredOrders.Add(order);
                }
            }
        }

        Log.Success(ClassName, $"Found {filteredOrders.Count} orders with status '{s}'.");
        return filteredOrders;
    }
}
