namespace OrderProcessing.CustomExceptions;

public class SmtpUnavailableException : Exception
{
    public SmtpUnavailableException(string message) : base(message) { }
}