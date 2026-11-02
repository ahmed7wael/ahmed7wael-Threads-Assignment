using OrderProcessing;
using OrderProcessing.Classes;

var jsonFileStore = new JsonFileStore();

var runner = new Phase1Runner(jsonFileStore);

await runner.RunAsync();

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();