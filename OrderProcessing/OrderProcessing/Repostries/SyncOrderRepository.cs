using OrderProcessing.Classes;
using OrderProcessing.Enums;

namespace OrderProcessing.Repostries;

public class SyncOrderRepository
{
    private readonly JsonFileStore _jsonFileStore;
    private const string ClassName = nameof(SyncOrderRepository);

    public SyncOrderRepository(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
    }

    public void Save(Order order, CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        Thread.Sleep(200);

        Log.Info(ClassName, "Beginning transaction...");
        Thread.Sleep(100);

        Log.Info(ClassName, $"Saving new Order #{order.Id}...");
        Thread.Sleep(400);

        var orders = _jsonFileStore.Read<Order>(DataFiles.orders, ct);
        if (orders == null)
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
        _jsonFileStore.Write(DataFiles.orders, orders, ct);

        Log.Info(ClassName, "Committing transaction...");
        Thread.Sleep(200);

        Log.Success(ClassName, $"Order #{order.Id} successfully saved.");
    }

    public List<Order> GetAll(CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        Thread.Sleep(200);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, "Executing query to get all orders...");
        Thread.Sleep(queryDelay);

        var orders = _jsonFileStore.Read<Order>(DataFiles.orders, ct);
        return orders ?? new List<Order>();
    }

    public List<Order> GetByStatus(OrderStatus s, CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        Thread.Sleep(200);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, $"Executing query for orders with status '{s}'...");
        Thread.Sleep(queryDelay);

        var allOrders = _jsonFileStore.Read<Order>(DataFiles.orders, ct);
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