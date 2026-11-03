using OrderProcessing.Classes;
using OrderProcessing.Enums;
using OrderProcessing.Repostries;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OrderProcessing;

public class OrderProcessorV2
{
    private readonly InventoryService _inventoryService;
    private readonly PricingService _pricingService;
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly InvoiceGenerator _invoiceGenerator;

    public OrderProcessorV2(InventoryService inventoryService, PricingService pricingService, OrderRepository orderRepository, ProductRepository productRepository, InvoiceGenerator invoiceGenerator)
    {
        _inventoryService = inventoryService;
        _pricingService = pricingService;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _invoiceGenerator = invoiceGenerator;
    }

    public async Task<bool> ProcessValidatedOrderAsync(Order order, CancellationToken ct = default)
    {
        var reservedItems = new List<OrderItem>();

        foreach (var item in order.Items)
        {
            bool isReserved = await _inventoryService.ReserveStockAsync(item.ProductId, item.Quantity, ct);
            if (!isReserved)
            {
                foreach (var reserved in reservedItems)
                {
                    await _inventoryService.ReleaseStockAsync(reserved.ProductId, reserved.Quantity, ct);
                }

                var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
                string productName = product != null ? product.Name : $"Product #{item.ProductId}";

                order.Status = OrderStatus.Rejected;
                order.FailureReason = $"Insufficient stock for '{productName}'";

                await _orderRepository.SaveAsync(order, ct);

                return false;
            }

            reservedItems.Add(item);
        }

        var pricingResult = await _pricingService.PriceOrderAsync(order, ct);
        await _invoiceGenerator.GenerateAsync(order, ct);

        order.TotalAmount = pricingResult.totalAmount;
        order.Status = OrderStatus.Completed;

        await _orderRepository.SaveAsync(order, ct);

        return true;
    }
}