namespace ical2s3grpc.Infrastructure.Storage;

public interface IStorageService
{
    Task<bool> SaveFileAsync(string fileName, string content, CancellationToken cancellationToken = default);
    Task<string?> GetFileAsync(string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default);
    Task<bool> BucketExistsAsync(CancellationToken cancellationToken = default);
}
