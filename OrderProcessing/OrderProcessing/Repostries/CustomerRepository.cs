using OrderProcessing.Classes;
using System.Security.AccessControl;

namespace OrderProcessing.Repostries;

public  class CustomerRepository
{
    private readonly JsonFileStore _jsonFileStore;
    private const string ClassName = nameof(CustomerRepository);

    public CustomerRepository(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
    }
    public async Task<Customer?> GetByIdAsync(int id , CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        await Task.Delay(200, ct);

        int QuaryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, $"Executing query for Customer #{id}...");
        await Task.Delay(QuaryDelay, ct);

        var customers = await _jsonFileStore.ReadAsync<Customer>(DataFiles.customers, ct);

        if(customers != null)
        {
            foreach (var customer in customers)
            {
                if (customer.Id == id)
                {
                    Log.Success(ClassName, $"Customer #{id} found ({customer.Name})");
                    return customer;
                }
            }
        }
        Log.Info(ClassName, $"Customer #{id} not found");
        return null;
    }
    public Customer? GetById(int id)
    {
        Log.Info(ClassName, "Opening connection...");
        Thread.Sleep(200);

        int QuaryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, $"Executing query for Customer #{id}...");
        Thread.Sleep(QuaryDelay);

        var customers = _jsonFileStore.Read<Customer>(DataFiles.customers);

        if (customers != null)
        {
            foreach (var customer in customers)
            {
                if (customer.Id == id)
                {
                    Log.Success(ClassName, $"Customer #{id} found ({customer.Name})");
                    return customer;
                }
            }
        }
        Log.Info(ClassName, $"Customer #{id} not found");
        return null;
    }
}
