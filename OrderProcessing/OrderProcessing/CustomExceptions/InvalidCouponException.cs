namespace OrderProcessing.Custom_Exptions;

public class InvalidCouponException : Exception
{
    public InvalidCouponException(string message) : base(message) { }
}