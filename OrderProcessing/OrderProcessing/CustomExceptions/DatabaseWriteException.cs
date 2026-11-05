namespace OrderProcessing.CustomExceptions;

public class DatabaseWriteException : Exception
{
    public DatabaseWriteException(string message) : base(message) { }
}