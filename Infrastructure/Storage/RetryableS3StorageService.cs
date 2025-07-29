using Amazon.S3;
using ical2s3grpc.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Polly;
using System.Diagnostics;

namespace ical2s3grpc.Infrastructure.Storage;

public class RetryableS3StorageService : IStorageService
{
    private readonly S3StorageService _innerService;
    private readonly ILogger<RetryableS3StorageService> _logger;
    private readonly ResiliencePipeline _retryPipeline;
    private static readonly ActivitySource ActivitySource = new("ical2s3grpc");

    public RetryableS3StorageService(
        IAmazonS3 s3Client,
        IOptions<S3Options> options,
        ILogger<RetryableS3StorageService> logger,
        IServiceProvider serviceProvider)
    {
        var s3Logger = serviceProvider.GetRequiredService<ILogger<S3StorageService>>();
        _innerService = new S3StorageService(s3Client, options, s3Logger);
        _logger = logger;

        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<AmazonS3Exception>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>(),
                Delay = TimeSpan.FromSeconds(1),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning("Retry attempt {AttemptNumber} for S3 operation due to: {Exception}",
                        args.AttemptNumber, args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<bool> SaveFileAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("S3.SaveFile");
        activity?.SetTag("s3.filename", fileName);
        activity?.SetTag("s3.content.length", content.Length);

        return await _retryPipeline.ExecuteAsync(async (ct) =>
        {
            var result = await _innerService.SaveFileAsync(fileName, content, ct);
            activity?.SetTag("s3.operation.success", result);
            return result;
        }, cancellationToken);
    }

    public async Task<string?> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("S3.GetFile");
        activity?.SetTag("s3.filename", fileName);

        return await _retryPipeline.ExecuteAsync(async (ct) =>
        {
            var result = await _innerService.GetFileAsync(fileName, ct);
            activity?.SetTag("s3.operation.success", result != null);
            if (result != null)
            {
                activity?.SetTag("s3.content.length", result.Length);
            }
            return result;
        }, cancellationToken);
    }

    public async Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("S3.DeleteFile");
        activity?.SetTag("s3.filename", fileName);

        return await _retryPipeline.ExecuteAsync(async (ct) =>
        {
            var result = await _innerService.DeleteFileAsync(fileName, ct);
            activity?.SetTag("s3.operation.success", result);
            return result;
        }, cancellationToken);
    }

    public async Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("S3.FileExists");
        activity?.SetTag("s3.filename", fileName);

        return await _retryPipeline.ExecuteAsync(async (ct) =>
        {
            var result = await _innerService.FileExistsAsync(fileName, ct);
            activity?.SetTag("s3.file.exists", result);
            return result;
        }, cancellationToken);
    }

    public async Task<bool> BucketExistsAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("S3.BucketExists");

        return await _retryPipeline.ExecuteAsync(async (ct) =>
        {
            var result = await _innerService.BucketExistsAsync(ct);
            activity?.SetTag("s3.bucket.exists", result);
            return result;
        }, cancellationToken);
    }
}
