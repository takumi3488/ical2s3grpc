namespace ical2s3grpc.Infrastructure.Configuration;

public class S3Options
{
    public const string SectionName = "S3";

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ServiceUrl { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public bool UseHttps { get; set; } = true;
    public bool ForcePathStyle { get; set; } = false;
}
