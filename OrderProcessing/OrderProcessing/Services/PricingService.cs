using System.Diagnostics;
using OrderProcessing.Classes;
using OrderProcessing.Custom_Exptions;
using OrderProcessing.Enums;
using OrderProcessing.Repostries;

namespace OrderProcessing.Services;

public class PricingService
{
    private readonly ProductRepository? _productRepository;
    private readonly CustomerRepository? _customerRepository;
    private readonly CouponRepository? _couponRepository;
    private readonly HashSet<int>? _failShippingForOrderIds;

    public PricingService(ProductRepository? productRepository, CustomerRepository? customerRepository, CouponRepository? couponRepository, IEnumerable<int>? failShippingForOrderIds)
    {
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _couponRepository = couponRepository;
        _failShippingForOrderIds = new HashSet<int>(failShippingForOrderIds ?? Enumerable.Empty<int>());
    }

    public async Task<decimal> CalculateSubTotalAsync(Order order, CancellationToken ct = default)
    {
        decimal subTotal = 0m;
        if (order?.Items != null)
        {
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
                if (product != null)
                {
                    item.UnitPrice = product.Price;
                    subTotal += item.UnitPrice * item.Quantity;
                }
            }
        }
        return Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<decimal> GetDiscountAsync(Order order, CancellationToken ct = default)
    {
        decimal discountPercentage = 0m;
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId, ct);
        if (customer != null)
        {
            discountPercentage += customer.LoyaltyTier switch
            {
                LoyaltyTier.Bronze => 0m,
                LoyaltyTier.Silver => 5m,
                LoyaltyTier.Gold => 10m,
                _ => 0m
            };
        }

        if (!string.IsNullOrWhiteSpace(order.CouponCode))
        {
            var coupon = await _couponRepository.GetByCodeAsync(order.CouponCode, ct);
            if (coupon == null)
            {
                throw new InvalidCouponException($"Coupon '{order.CouponCode}' is invalid or does not exist.");
            }
            discountPercentage += coupon.DiscountPercent;
        }

        if (discountPercentage > 30m)
            discountPercentage = 30m;

        decimal subTotal = await CalculateSubTotalAsync(order, ct);
        decimal discountAmount = subTotal * (discountPercentage / 100m);

        return Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<decimal> CalculateShippingCostAsync(Order order, CancellationToken ct = default)
    {
        if (order != null && _failShippingForOrderIds != null && _failShippingForOrderIds.Contains(order.Id))
        {
            throw new ShippingProviderUnavailableException("Shipping provider unavailable");
        }

        decimal subTotal = await CalculateSubTotalAsync(order, ct);
        if (subTotal >= 1000m)
        {
            return 0m;
        }

        decimal totalWeight = 0m;
        int totalQuantity = 0;

        if (order?.Items != null)
        {
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
                if (product != null)
                {
                    totalQuantity += item.Quantity;
                    totalWeight += product.WeightKg * item.Quantity;
                }
            }
        }

        decimal shippingCost = 5.00m + (1.50m * totalQuantity) + (2.00m * totalWeight);
        return Math.Round(shippingCost, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<(decimal subTotal, decimal discount, decimal shipping, decimal totalAmount, long elapsedMs)> PriceOrderAsync(Order order, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        decimal subTotal = await CalculateSubTotalAsync(order, ct);

        var discountTask = GetDiscountAsync(order, ct);
        var shippingTask = CalculateShippingCostAsync(order, ct);

        decimal discount = await discountTask;
        decimal shipping = await shippingTask;

        decimal totalAmount = Math.Round(subTotal - discount + shipping, 2, MidpointRounding.AwayFromZero);

        stopwatch.Stop();
        return (subTotal, discount, shipping, totalAmount, stopwatch.ElapsedMilliseconds);
    }

    public async Task<(decimal subTotal, decimal discount, decimal shipping, decimal totalAmount, long elapsedMs)> PriceOrderSequentialAsync(Order order, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        decimal subTotal = await CalculateSubTotalAsync(order, ct);
        decimal discount = await GetDiscountAsync(order, ct);
        decimal shipping = await CalculateShippingCostAsync(order, ct);

        decimal totalAmount = Math.Round(subTotal - discount + shipping, 2, MidpointRounding.AwayFromZero);

        stopwatch.Stop();
        return (subTotal, discount, shipping, totalAmount, stopwatch.ElapsedMilliseconds);
    }
    public decimal CalculateSubTotalSync(Order order)
    {
        decimal subTotal = 0m;
        if (order?.Items != null)
        {
            foreach (var item in order.Items)
            {
                var product = _productRepository?.GetById(item.ProductId);
                if (product != null)
                {
                    item.UnitPrice = product.Price;
                    subTotal += item.UnitPrice * item.Quantity;
                }
            }
        }
        return Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);
    }

    public decimal GetDiscountSync(Order order)
    {
        decimal discountPercentage = 0m;
        var customer = _customerRepository?.GetById(order.CustomerId);
        if (customer != null)
        {
            discountPercentage += customer.LoyaltyTier switch
            {
                LoyaltyTier.Bronze => 0m,
                LoyaltyTier.Silver => 5m,
                LoyaltyTier.Gold => 10m,
                _ => 0m
            };
        }

        if (!string.IsNullOrWhiteSpace(order.CouponCode))
        {
            var coupon = _couponRepository?.GetByCode(order.CouponCode);
            if (coupon == null)
            {
                throw new InvalidCouponException($"Coupon '{order.CouponCode}' is invalid or does not exist.");
            }
            discountPercentage += coupon.DiscountPercent;
        }

        if (discountPercentage > 30m)
            discountPercentage = 30m;

        decimal subTotal = CalculateSubTotalSync(order);
        decimal discountAmount = subTotal * (discountPercentage / 100m);

        return Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero);
    }

    public decimal CalculateShippingCostSync(Order order)
    {
        if (order != null && _failShippingForOrderIds != null && _failShippingForOrderIds.Contains(order.Id))
        {
            throw new ShippingProviderUnavailableException("Shipping provider unavailable");
        }

        decimal subTotal = CalculateSubTotalSync(order);
        if (subTotal >= 1000m)
        {
            return 0m;
        }

        decimal totalWeight = 0m;
        int totalQuantity = 0;

        if (order?.Items != null)
        {
            foreach (var item in order.Items)
            {
                var product = _productRepository?.GetById(item.ProductId);
                if (product != null)
                {
                    totalQuantity += item.Quantity;
                    totalWeight += product.WeightKg * item.Quantity;
                }
            }
        }

        decimal shippingCost = 5.00m + (1.50m * totalQuantity) + (2.00m * totalWeight);
        return Math.Round(shippingCost, 2, MidpointRounding.AwayFromZero);
    }

    public (decimal subTotal, decimal discount, decimal shipping, decimal totalAmount, long elapsedMs) PriceOrderSync(Order order)
    {
        var stopwatch = Stopwatch.StartNew();
        decimal subTotal = CalculateSubTotalSync(order);
        decimal discount = GetDiscountSync(order);
        decimal shipping = CalculateShippingCostSync(order);

        decimal totalAmount = Math.Round(subTotal - discount + shipping, 2, MidpointRounding.AwayFromZero);

        stopwatch.Stop();
        return (subTotal, discount, shipping, totalAmount, stopwatch.ElapsedMilliseconds);
    }
}