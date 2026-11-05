using OrderProcessing.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.interfaces;

public interface IShippingCarrier
{
    string Name { get; }
    Task<decimal> GetQuoteAsync(Order order, decimal baseShippingCost, CancellationToken ct);
}
