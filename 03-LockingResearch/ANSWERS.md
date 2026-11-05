# Assignment 03 — Answers

Student name: ahmed wael 
Group / ID: Group 4

1.System.Threading.Monitor : ده الكلاس اللي بيعمل Thread Synchorouns يعني تنظيم مين يدخل علي جزء ال Critical section ومين يستني عن طريق 2 Method is .Enter and .ُExit 

2. compile to : .Enter للدخول عن طريق ال Object والجزء ال Critical in try and .Exit in finally to release the lock

3. 1 - lock is easy to use and more readable 
   2 - excaption safety do not write finally to release lock 
   3 - more functions in monitor to Flexibility such 

4. Monitor.TryEnter : لو عايز ادي ال thread timeout لو خلص يروح ميستناش ويعمل anthor jobs 
   Monitor.Wait : ال thread ده بيفضل مستني شرط معين عشان يتحقق وبعدين يشتغل 
   Monitor.Pulse : ده بيبعت notification to thread use .wait عشان يقوله ان الشرط اتحقق ويبدا شغل 

5. # lock : 
   private readonly object _sync = new();

   public void SafeOperationWithLock()
    {
        lock (_sync)
        {
        Console.WriteLine("Executing inside lock block.");
        }
    }

    # Monitor 
    private readonly object _sync = new();

public void SafeOperationWithMonitor()
{
    bool lockTaken = false;
    try
    {
        Monitor.Enter(_sync, ref lockTaken);
        Console.WriteLine("Executing inside Monitor block.");
    }
    finally
    {
        if (lockTaken)
        {
            Monitor.Exit(_sync);
        }
    }
}


Links:

-https://learn.microsoft.com/en-us/dotnet/api/system.threading.monitor?view=net-10.0 
وفهمت كل حاجه بردو من شات يعني فهمني كل حاجه بطريقة افضل وابسط 
