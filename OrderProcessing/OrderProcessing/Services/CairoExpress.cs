using OrderProcessing.Classes;
using OrderProcessing.Custom_Exptions;
using OrderProcessing.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.Services;

public class CairoExpress : IShippingCarrier
{
    public string Name => "CairoExpress";
    private readonly HashSet<int> _failForOrderIds;

    public CairoExpress(IEnumerable<int> failForOrderIds = null)
    {
        _failForOrderIds = new HashSet<int>();
        if (failForOrderIds != null)
        {
            foreach (var id in failForOrderIds)
            {
                _failForOrderIds.Add(id);
            }
        }
    }

    public async Task<decimal> GetQuoteAsync(Order order, decimal baseShippingCost, CancellationToken ct)
    {
        await Task.Delay(150, ct);
        int randomTime = Random.Shared.Next(300, 2001);
        await Task.Delay(randomTime, ct);

        // Built-in failure: ترمي استثناء للأوردرات الزوجية أو المعرّفة في الفشل
        if (order.Id % 2 == 0 || _failForOrderIds.Contains(order.Id))
        {
            throw new ShippingProviderUnavailableException($"CairoExpress failed for order #{order.Id}");
        }

        // Price = baseShippingCost * 0.95
        return baseShippingCost * 0.95m;
    }
}