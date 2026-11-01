using System.Text.Json.Serialization;

namespace OrderProcessing.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Pending,
    Validated,
    Rejected,
    PaymentFailed,
    Completed,
    Failed
}
