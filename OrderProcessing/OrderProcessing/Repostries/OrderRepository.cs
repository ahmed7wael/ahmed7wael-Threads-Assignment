using OrderProcessing.Classes;
using OrderProcessing.Enums;
// تأكد من استيراد الـ namespace الخاص بالـ Custom Exceptions لو مش موجود عندك
using OrderProcessing.CustomExceptions;

namespace OrderProcessing.Repostries;

public class OrderRepository
{
    private readonly JsonFileStore _jsonFileStore;
    private const string ClassName = nameof(OrderRepository);
    private readonly HashSet<int> _failSaveOrderIds;

    public OrderRepository(JsonFileStore jsonFileStore, IEnumerable<int>? failSaveOrderIds = null)
    {
        _jsonFileStore = jsonFileStore;
        _failSaveOrderIds = new HashSet<int>();

        if (failSaveOrderIds != null)
        {
            foreach (var id in failSaveOrderIds)
            {
                _failSaveOrderIds.Add(id);
            }
        }
    }

    public async Task SaveAsync(Order order, CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        await Task.Delay(200, ct);

        Log.Info(ClassName, "Beginning transaction...");
        await Task.Delay(100, ct);

        Log.Info(ClassName, $"Saving new Order #{order.Id}...");
        await Task.Delay(400, ct);

        var orders = await _jsonFileStore.ReadAsync<Order>(DataFiles.orders, ct);
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

        if (_failSaveOrderIds != null && _failSaveOrderIds.Contains(order.Id) && order.Status == OrderStatus.Completed)
        {
            Log.Error(ClassName, $"Database write failed for Order #{order.Id}.");
            throw new DatabaseWriteException("Connection lost during commit"); 
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
    public void SaveSync(Order order)
    {
        Log.Info(ClassName, "Opening connection...");
        Thread.Sleep(200);

        Log.Info(ClassName, "Beginning transaction...");
        Thread.Sleep(100);

        Log.Info(ClassName, $"Saving new Order #{order.Id}...");
        Thread.Sleep(400);

        var orders = _jsonFileStore.Read<Order>(DataFiles.orders);
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

        if (_failSaveOrderIds != null && _failSaveOrderIds.Contains(order.Id) && order.Status == OrderStatus.Completed)
        {
            Log.Error(ClassName, $"Database write failed for Order #{order.Id}.");
            throw new DatabaseWriteException("Connection lost during commit");
        }

        _jsonFileStore.Write(DataFiles.orders, orders);

        Log.Info(ClassName, "Committing transaction...");
        Thread.Sleep(200);

        Log.Success(ClassName, $"Order #{order.Id} successfully saved.");
    }

    public List<Order> GetAllSync()
    {
        Log.Info(ClassName, "Opening connection...");
        Thread.Sleep(200);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, "Executing query to get all orders...");
        Thread.Sleep(queryDelay);

        var orders = _jsonFileStore.Read<Order>(DataFiles.orders);
        return orders ?? new List<Order>();
    }

    public List<Order> GetByStatusSync(OrderStatus s)
    {
        Log.Info(ClassName, "Opening connection...");
        Thread.Sleep(200);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, $"Executing query for orders with status '{s}'...");
        Thread.Sleep(queryDelay);

        var allOrders = _jsonFileStore.Read<Order>(DataFiles.orders);
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