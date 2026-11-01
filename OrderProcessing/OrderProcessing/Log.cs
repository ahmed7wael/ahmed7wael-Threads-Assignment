namespace OrderProcessing;

public  static class Log
{
    private static readonly Lock _lock = new();
    public static void Info(string className , string massage)
    {
        WriteLog(className , massage , ConsoleColor.Gray);
    }

    public static void Success(string className, string massage)
    {
        WriteLog(className, massage, ConsoleColor.Green);
    }

    public static void Error(string className, string massage)
    {
        WriteLog(className, massage, ConsoleColor.Red);
    }
    private static void WriteLog(string className, string massage , ConsoleColor color)
    {
        lock (_lock)
        {
            try
            {
                var time = DateTime.Now.ToString("HH:mm:ss.fff");
                var threadId = Environment.CurrentManagedThreadId;

                Console.ForegroundColor = color;

                Console.WriteLine($"[{time}] [Thread {threadId}] {className} -> {massage}");
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }

}
