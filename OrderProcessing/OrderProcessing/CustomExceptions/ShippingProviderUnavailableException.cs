namespace OrderProcessing.Custom_Exptions;

public class ShippingProviderUnavailableException : Exception
{
    public ShippingProviderUnavailableException(string massage) : base(massage)
    {

    }
}
