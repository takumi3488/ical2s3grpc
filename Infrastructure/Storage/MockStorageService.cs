using System.Collections.Concurrent;

namespace ical2s3grpc.Infrastructure.Storage;

public class MockStorageService : IStorageService
{
    private readonly ConcurrentDictionary<string, string> _storage = new();

    public Task<bool> SaveFileAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        _storage.AddOrUpdate(fileName, content, (key, oldValue) => content);
        return Task.FromResult(true);
    }

    public Task<string?> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        _storage.TryGetValue(fileName, out var content);
        return Task.FromResult(content);
    }

    public Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var removed = _storage.TryRemove(fileName, out _);
        return Task.FromResult(removed);
    }

    public Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var exists = _storage.ContainsKey(fileName);
        return Task.FromResult(exists);
    }

    public Task<bool> BucketExistsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public void Clear()
    {
        _storage.Clear();
    }

    public IReadOnlyDictionary<string, string> GetAllFiles()
    {
        return _storage.ToArray().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}