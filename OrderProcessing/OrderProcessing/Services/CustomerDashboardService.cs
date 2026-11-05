using OrderProcessing.Classes;
using OrderProcessing.CustomExceptions;
using OrderProcessing.Repostries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OrderProcessing.Services;

public class CustomerDashboardService
{
    private readonly CustomerRepository _customerRepository;
    private readonly OrderRepository _orderRepository;
    private readonly JsonFileStore _jsonFileStore;
    private readonly HashSet<int> _failPaymentsForCustomerIds;

    public CustomerDashboardService(
        CustomerRepository customerRepository,
        OrderRepository orderRepository,
        JsonFileStore jsonFileStore,
        IEnumerable<int> failPaymentsForCustomerIds = null)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _jsonFileStore = jsonFileStore;

        _failPaymentsForCustomerIds = new HashSet<int>();
        if (failPaymentsForCustomerIds != null)
        {
            foreach (var id in failPaymentsForCustomerIds)
            {
                _failPaymentsForCustomerIds.Add(id);
            }
        }
    }

    public async Task<CustomerDashboard> LoadAsync(int customerId, CancellationToken ct = default)
    {
        // R2: إطلاق الأربع مهام معاً في نفس اللحظة
        Task<Customer> customerTask = GetCustomerProfileAsync(customerId, ct);
        Task<List<Order>> ordersTask = GetCustomerOrdersAsync(customerId, ct);
        Task<List<PaymentTransaction>> paymentsTask = GetCustomerPaymentsAsync(customerId, ct);
        Task<List<EmailMessage>> emailsTask = GetCustomerEmailsAsync(customerId, ct);

        Task allTasks = Task.WhenAll(customerTask, ordersTask, paymentsTask, emailsTask);

        try
        {
            // R2: الانتظار بـ await واحدة فقط
            await allTasks;
        }
        catch (Exception)
        {
            // R4: التقاط كل الأخطاء الناتجة من AggregateException
            if (allTasks.Exception != null)
            {
                throw allTasks.Exception;
            }
            throw;
        }

        return new CustomerDashboard
        {
            Customer = await customerTask,
            Orders = await ordersTask,
            Payments = await paymentsTask,
            Emails = await emailsTask
        };
    }

    private async Task<Customer> GetCustomerProfileAsync(int customerId, CancellationToken ct)
    {
        await Task.Delay(150, ct); // محاكاة وقت الداتا بيز
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        if (customer == null)
        {
            throw new CustomerNotFoundException($"Customer #{customerId} not found");
        }
        return customer;
    }

    private async Task<List<Order>> GetCustomerOrdersAsync(int customerId, CancellationToken ct)
    {
        await Task.Delay(200, ct); // محاكاة وقت الداتا بيز
        var allOrders = await _orderRepository.GetAllAsync(ct);
        var customerOrders = new List<Order>();

        if (allOrders != null)
        {
            foreach (var order in allOrders)
            {
                if (order.CustomerId == customerId)
                {
                    customerOrders.Add(order);
                }
            }
        }
        return customerOrders;
    }

    private async Task<List<PaymentTransaction>> GetCustomerPaymentsAsync(int customerId, CancellationToken ct)
    {
        await Task.Delay(250, ct);

        if (_failPaymentsForCustomerIds.Contains(customerId))
        {
            throw new TimeoutException("Payments store timeout");
        }

        var allOrders = await _orderRepository.GetAllAsync(ct);
        var customerOrderIds = new HashSet<int>();
        if (allOrders != null)
        {
            foreach (var order in allOrders)
            {
                if (order.CustomerId == customerId)
                {
                    customerOrderIds.Add(order.Id);
                }
            }
        }

        var allPayments = await _jsonFileStore.ReadAsync<PaymentTransaction>(DataFiles.payments, ct);
        var customerPayments = new List<PaymentTransaction>();

        if (allPayments != null)
        {
            foreach (var payment in allPayments)
            {
                if (customerOrderIds.Contains(payment.OrderId))
                {
                    customerPayments.Add(payment);
                }
            }
        }
        return customerPayments;
    }

    private async Task<List<EmailMessage>> GetCustomerEmailsAsync(int customerId, CancellationToken ct)
    {
        await Task.Delay(180, ct);

        var allOrders = await _orderRepository.GetAllAsync(ct);
        var customerOrderIds = new HashSet<int>();
        if (allOrders != null)
        {
            foreach (var order in allOrders)
            {
                if (order.CustomerId == customerId)
                {
                    customerOrderIds.Add(order.Id);
                }
            }
        }

        var allEmails = await _jsonFileStore.ReadAsync<EmailMessage>(DataFiles.emails, ct);
        var customerEmails = new List<EmailMessage>();

        if (allEmails != null)
        {
            foreach (var email in allEmails)
            {
                if (customerOrderIds.Contains(email.OrderId))
                {
                    customerEmails.Add(email);
                }
            }
        }
        return customerEmails;
    }
}