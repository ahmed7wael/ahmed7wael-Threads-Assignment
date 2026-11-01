using System.Collections.Concurrent;
using System.Text.Json;

namespace OrderProcessing;

public class JsonFileStore
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();
    private static SemaphoreSlim GetSemaphore(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        return _semaphores.GetOrAdd(fullPath, _ => new SemaphoreSlim(1, 1));
    }
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public async Task<List<T>> ReadAsync<T>(string filePath , CancellationToken cancellationToken = default)
    {
        var semaphore = GetSemaphore(filePath);
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(filePath))
                return new List<T>();
            var json = await File.ReadAllTextAsync(filePath , cancellationToken);

            List<T>? data;
            try
            {
                data = JsonSerializer.Deserialize<List<T>>(json, _options);
            }
            catch(JsonException ex)
            {
                throw new DataFileCorruptedException(filePath, "The JSON file contains invalid data.", ex);
            }
            if (data is null)
            {
                throw new DataFileCorruptedException(Path.GetFileName(filePath),"The JSON file does not contain a valid collection." , null);
            }

            return data;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task WriteAsync<T>(string filePath , List<T> data , CancellationToken cancellationToken = default)
    {
        var semaphore = GetSemaphore(filePath);
        await semaphore.WaitAsync(cancellationToken);

        string tempFilePath = filePath + ".tmp";
        try
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(data, _options);

            await File.WriteAllTextAsync(tempFilePath, json , cancellationToken);

            File.Move(tempFilePath, filePath, overwrite: true);
        }
        finally
        {
            try
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
            finally
            {
                semaphore.Release();
            }
        }

    }
}
