using OrderProcessing.Classes;
using OrderProcessing.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.Services;

public class FastShip : IShippingCarrier
{
    public string Name => "FastShip";

    public async Task<decimal> GetQuoteAsync(Order order, decimal baseShippingCost, CancellationToken ct)
    {
        await Task.Delay(150, ct);
        int randomTime = Random.Shared.Next(500, 1501);
        await Task.Delay(randomTime, ct);

        return baseShippingCost * 1.00m;
    }
}
