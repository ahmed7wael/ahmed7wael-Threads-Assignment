using OrderProcessing.Classes;
using OrderProcessing.CustomExceptions;
using OrderProcessing.Repostries;

namespace OrderProcessing;

public class CustomerService
{
    private readonly CustomerRepository _customerRepository;
    private const string ClassName = nameof(CustomerService);

    public CustomerService(CustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer> GetCustomerAsync(int id, CancellationToken ct = default)
    {
        Log.Info(ClassName, $"Fetching customer #{id}...");

        var customer = await _customerRepository.GetByIdAsync(id, ct);

        if (id == 99)
        {
            Log.Error(ClassName, "Customer #99 not found.");
            throw new CustomerNotFoundException("Customer #99 not found");
        }

        if (customer == null)
        {
            Log.Error(ClassName, $"Customer #{id} was not found.");
            throw new CustomerNotFoundException($"Customer with ID {id} does not exist.");
        }

        Log.Success(ClassName, $"Customer #{id} successfully retrieved by service.");
        return customer;
    }
    public Customer GetCustomerSync(int id)
    {
        Log.Info(ClassName, $"Fetching customer #{id}...");

        var customer = _customerRepository.GetById(id);

        if (id == 99)
        {
            Log.Error(ClassName, "Customer #99 not found.");
            throw new CustomerNotFoundException("Customer #99 not found");
        }

        if (customer == null)
        {
            Log.Error(ClassName, $"Customer #{id} was not found.");
            throw new CustomerNotFoundException($"Customer with ID {id} does not exist.");
        }

        Log.Success(ClassName, $"Customer #{id} successfully retrieved by service.");
        return customer;
    }
}