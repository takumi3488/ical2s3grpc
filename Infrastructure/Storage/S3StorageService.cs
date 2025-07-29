using Amazon.S3;
using Amazon.S3.Model;
using ical2s3grpc.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Text;

namespace ical2s3grpc.Infrastructure.Storage;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _options;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IAmazonS3 s3Client, IOptions<S3Options> options, ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SaveFileAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Attempting to save file {FileName} to S3 bucket {BucketName}", fileName, _options.BucketName);

        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileName,
                ContentBody = content,
                ContentType = "text/calendar"
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);
            var success = response.HttpStatusCode == System.Net.HttpStatusCode.OK;

            if (success)
            {
                _logger.LogInformation("Successfully saved file {FileName} to S3 bucket {BucketName}. ETag: {ETag}",
                    fileName, _options.BucketName, response.ETag);
            }
            else
            {
                _logger.LogWarning("S3 PutObject returned unexpected status code {StatusCode} for file {FileName}",
                    response.HttpStatusCode, fileName);
            }

            return success;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error while saving file {FileName} to bucket {BucketName}. Error Code: {ErrorCode}, Status Code: {StatusCode}",
                fileName, _options.BucketName, ex.ErrorCode, ex.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving file {FileName} to S3 bucket {BucketName}",
                fileName, _options.BucketName);
            return false;
        }
    }

    public async Task<string?> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Attempting to get file {FileName} from S3 bucket {BucketName}", fileName, _options.BucketName);

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileName
            };

            using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            using var reader = new StreamReader(response.ResponseStream);
            var content = await reader.ReadToEndAsync();

            _logger.LogDebug("Successfully retrieved file {FileName} from S3 bucket {BucketName}. Size: {Size} bytes",
                fileName, _options.BucketName, content.Length);

            return content;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogDebug("File {FileName} not found in S3 bucket {BucketName}", fileName, _options.BucketName);
            return null;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error while getting file {FileName} from bucket {BucketName}. Error Code: {ErrorCode}, Status Code: {StatusCode}",
                fileName, _options.BucketName, ex.ErrorCode, ex.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting file {FileName} from S3 bucket {BucketName}",
                fileName, _options.BucketName);
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileName
            };

            var response = await _s3Client.DeleteObjectAsync(request, cancellationToken);
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _options.BucketName,
                Key = fileName
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> BucketExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.GetBucketLocationAsync(_options.BucketName, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
