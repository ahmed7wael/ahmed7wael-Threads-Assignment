using System.Text.Json.Serialization;

namespace OrderProcessing.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LoyaltyTier
{
    Bronze,
    Silver,
    Gold
}