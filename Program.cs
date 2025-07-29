using Amazon.S3;
using ical2s3grpc.Infrastructure.Configuration;
using ical2s3grpc.Infrastructure.Storage;
using ical2s3grpc.Services;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for gRPC
builder.WebHost.ConfigureKestrel(options =>
{
    // This endpoint will use HTTP/2 for gRPC
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });

    // Add a separate endpoint for HTTP/1.x health checks
    options.ListenAnyIP(8081, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

// Add configuration - S3 settings from environment variables only
builder.Services.Configure<S3Options>(options =>
{
    options.AccessKey = builder.Configuration["S3_ACCESS_KEY"] ?? throw new InvalidOperationException("S3_ACCESS_KEY environment variable is required");
    options.SecretKey = builder.Configuration["S3_SECRET_KEY"] ?? throw new InvalidOperationException("S3_SECRET_KEY environment variable is required");
    options.ServiceUrl = builder.Configuration["S3_SERVICE_URL"] ?? string.Empty; // Empty for AWS S3, required for MinIO/custom endpoints
    options.BucketName = builder.Configuration["S3_BUCKET_NAME"] ?? throw new InvalidOperationException("S3_BUCKET_NAME environment variable is required");
    options.Region = builder.Configuration["S3_REGION"] ?? "us-east-1";

    // Validate boolean configurations
    if (!bool.TryParse(builder.Configuration["S3_USE_HTTPS"] ?? "true", out var useHttps))
    {
        throw new InvalidOperationException("S3_USE_HTTPS must be 'true' or 'false'");
    }
    options.UseHttps = useHttps;

    if (!bool.TryParse(builder.Configuration["S3_FORCE_PATH_STYLE"] ?? "false", out var forcePathStyle))
    {
        throw new InvalidOperationException("S3_FORCE_PATH_STYLE must be 'true' or 'false'");
    }
    options.ForcePathStyle = forcePathStyle;

    // Validate bucket name format
    if (string.IsNullOrWhiteSpace(options.BucketName) ||
        options.BucketName.Length < 3 ||
        options.BucketName.Length > 63)
    {
        throw new InvalidOperationException("S3_BUCKET_NAME must be between 3 and 63 characters");
    }
});

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation(options =>
            {
                options.Filter = (httpContext) =>
                {
                    // Exclude health check endpoints from tracing
                    return !httpContext.Request.Path.StartsWithSegments("/health");
                };
            })
            .AddHttpClientInstrumentation()
            .AddGrpcClientInstrumentation();

        // Use OTLP exporter for traces
        tracing.AddOtlpExporter(options =>
        {
            options.Protocol = OtlpExportProtocol.Grpc;
        });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation();

        // Use OTLP exporter for metrics
        metrics.AddOtlpExporter(options =>
        {
            options.Protocol = OtlpExportProtocol.Grpc;
        });
    });

// Add application services
builder.Services.AddScoped<ICalendarGenerator, CalendarGenerator>();

// Add storage services
if (builder.Environment.IsDevelopment())
{
    // Use mock storage in development
    builder.Services.AddSingleton<IStorageService, MockStorageService>();
}
else
{
    // Use real S3 in production
    builder.Services.AddScoped(provider =>
    {
        var s3Options = provider.GetRequiredService<IOptions<S3Options>>().Value;
        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(s3Options.Region),
            UseHttp = !s3Options.UseHttps,
            ForcePathStyle = s3Options.ForcePathStyle
        };

        // Only set ServiceURL for custom endpoints (MinIO, etc.)
        if (!string.IsNullOrEmpty(s3Options.ServiceUrl))
        {
            config.ServiceURL = s3Options.ServiceUrl;
        }

        IAmazonS3 client = new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, config);
        return client;
    });
    builder.Services.AddScoped<IStorageService, RetryableS3StorageService>();
}

var app = builder.Build();

// Validate bucket existence at startup (only in production)
if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var s3Options = scope.ServiceProvider.GetRequiredService<IOptions<S3Options>>().Value;

    logger.LogInformation("Validating S3 bucket '{BucketName}' exists...", s3Options.BucketName);

    try
    {
        var bucketExists = await storageService.BucketExistsAsync();
        if (!bucketExists)
        {
            logger.LogError("S3 bucket '{BucketName}' does not exist. Please create the bucket before starting the application.", s3Options.BucketName);
            throw new InvalidOperationException($"S3 bucket '{s3Options.BucketName}' does not exist.");
        }

        logger.LogInformation("S3 bucket '{BucketName}' validated successfully.", s3Options.BucketName);
    }
    catch (Exception ex) when (!(ex is InvalidOperationException))
    {
        logger.LogError(ex, "Failed to validate S3 bucket '{BucketName}'. Check your S3 configuration and network connectivity.", s3Options.BucketName);
        throw new InvalidOperationException($"Failed to validate S3 bucket '{s3Options.BucketName}'. Check your S3 configuration and network connectivity.", ex);
    }
}

// Configure the HTTP request pipeline.
app.MapGrpcService<ICalendarServiceImpl>();
app.MapGrpcReflectionService();

// Map health endpoint only to HTTP/1.x port
app.MapWhen(context => context.Connection.LocalPort == 8081,
    builder =>
    {
        builder.Use(async (context, next) =>
        {
            if (context.Request.Path == "/health/ready")
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            }
            else
            {
                await next();
            }
        });
    });

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
