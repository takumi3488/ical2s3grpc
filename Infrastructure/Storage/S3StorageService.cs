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

    public S3StorageService(IAmazonS3 s3Client, IOptions<S3Options> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<bool> SaveFileAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileName,
                ContentBody = content,
                ContentType = "text/calendar",
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<string?> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileName
            };

            using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception)
        {
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