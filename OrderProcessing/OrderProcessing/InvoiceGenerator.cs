using OrderProcessing.Classes;

namespace OrderProcessing;

public class InvoiceGenerator
{
    public async Task GenerateAsync(Order order, CancellationToken ct = default)
    {
        await Task.Run(() =>
        {
            long dummyCalculation = 0;
            for (int i = 0; i < 20_000_000; i++)
            {
                dummyCalculation += i % 7;
            }

            Directory.CreateDirectory(DataFiles.InvoicesFolder);

            var filePath = Path.Combine(DataFiles.InvoicesFolder, $"invoice_{order.Id}.txt");
            File.WriteAllText(filePath, $"Invoice for Order #{order.Id}\nChecksum: {dummyCalculation}");
        }, ct);
    }
}
