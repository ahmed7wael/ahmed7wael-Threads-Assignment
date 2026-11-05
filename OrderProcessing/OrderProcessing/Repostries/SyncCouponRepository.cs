using NPOI.SS.Formula.Functions;
using OrderProcessing.Classes;

namespace OrderProcessing.Repostries;

public class SyncCouponRepository
{
    private readonly JsonFileStore _jsonFileStore;
    private const string ClassName = nameof(SyncCouponRepository);

    public SyncCouponRepository(JsonFileStore jsonFileStore)
    {
        _jsonFileStore = jsonFileStore;
    }

    public Coupon? GetByCode(string code, CancellationToken ct = default)
    {
        Log.Info(ClassName, "Opening connection...");
        Thread.Sleep(200);

        int quaryDelay = Random.Shared.Next(300, 601);
        Log.Info(ClassName, $"Executing query for Coupon '{code}'...");
        Thread.Sleep(quaryDelay);

        var coupons = _jsonFileStore.Read<Coupon>(DataFiles.coupons, ct);
        if (coupons != null)
        {
            foreach (var coupon in coupons)
            {
                if (string.Equals(coupon.Code, code, StringComparison.OrdinalIgnoreCase))
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