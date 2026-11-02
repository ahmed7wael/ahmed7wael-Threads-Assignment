using NPOI.SS.Formula.Functions;
using OrderProcessing.Classes;

namespace OrderProcessing.Repostries;

public class CouponRepository
{
    private readonly JsonFileStore _jsonFileStore;
    private const string ClassName = nameof(CouponRepository);

    public CouponRepository(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
    }
    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        await Task.Delay(200, ct);

        int quaryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, $"Executing query for Coupon '{code}'...");
        await Task.Delay(quaryDelay, ct);

        var coupons = await _jsonFileStore.ReadAsync<Coupon>(DataFiles.coupons, ct);
        if(coupons != null)
        {
            foreach(var coupon in coupons)
            {
                if(string.Equals(coupon.Code , code , StringComparison.OrdinalIgnoreCase))
                {
                    Log.Success(ClassName, $"Coupon '{code}' found.");
                    return coupon;
                }
            }
        }
        Log.Info(ClassName, $"Coupon '{code}' not found.");
        return null;
    }
}
