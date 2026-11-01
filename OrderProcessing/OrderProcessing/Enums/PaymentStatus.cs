using System.Text.Json.Serialization;

namespace OrderProcessing.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Captured,
    Declined,
    Refunded
}

