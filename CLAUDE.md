# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is an incomplete gRPC service that converts events to iCalendar format and saves them to S3. It is built with C# for all components.

## Build and Run Commands

### Environment Variables

All application configuration is done via environment variables:

#### S3 Configuration (Required)
```bash
export S3_ACCESS_KEY="your-access-key"           # S3 access key credential
export S3_SECRET_KEY="your-secret-key"           # S3 secret key credential
export S3_BUCKET_NAME="your-bucket-name"         # Target S3 bucket name
```

#### S3 Configuration (Optional)
```bash
export S3_SERVICE_URL=""                         # S3 endpoint URL (empty for AWS S3, required for MinIO/custom)
export S3_REGION="us-east-1"                     # AWS region (default: us-east-1)
export S3_USE_HTTPS="true"                       # Use HTTPS for S3 connections (default: true)
export S3_FORCE_PATH_STYLE="false"               # Use path-style addressing (default: false for AWS)
```

#### OpenTelemetry Configuration (Optional)
```bash
export OTEL_EXPORTER_OTLP_ENDPOINT=""            # OTLP endpoint (e.g., http://localhost:4317)
export OTEL_SERVICE_NAME="ical2s3grpc"           # Service name for tracing (default: ical2s3grpc)
```

### Running the Application

```bash
# Run locally with AWS S3
export S3_ACCESS_KEY="your-aws-access-key"
export S3_SECRET_KEY="your-aws-secret-key"
export S3_BUCKET_NAME="my-ical-bucket"
dotnet run

# Run locally with MinIO
export S3_ACCESS_KEY="admin"
export S3_SECRET_KEY="minio123"
export S3_SERVICE_URL="http://localhost:9000"
export S3_BUCKET_NAME="ical2s3grpc"
export S3_USE_HTTPS="false"
export S3_FORCE_PATH_STYLE="true"
dotnet run

# Build the project
dotnet build

# Run with Docker Compose (includes MinIO, Jaeger)
docker compose up

# Run tests with task
task ci        # restore, build, format and test
task test      # integration test with docker compose
task dev       # development mode with local MinIO
```

### Configuration Notes

**AWS S3 vs MinIO Settings:**
- **AWS S3**: Leave `S3_SERVICE_URL` empty, use default settings
- **MinIO**: Requires `S3_SERVICE_URL`, `S3_FORCE_PATH_STYLE=true`, and typically `S3_USE_HTTPS=false`

**Docker Compose Environment:**
- Automatically sets up MinIO on port 9000 (console on 9001)
- Configures Jaeger for tracing on port 16686
- Pre-creates the S3 bucket via s3-init service
- All services are health-checked before dependencies start

## Architecture

The project uses:
- **C#** for all components including gRPC services, S3 operations, and business logic
- **gRPC** for service interface (currently using greet.proto but should be replaced with ical2s3.proto)
- **MinIO** as local S3 storage in development
- **OpenTelemetry** for distributed tracing and metrics
- **Jaeger** for trace visualization in development (accessible at http://localhost:16686)

## Current State and TODO

The project is incomplete. Based on instruction.md, the following changes need to be made:
- Rename `ConvertToiCalendar` to `SaveEvents`
- Rename `EventRequest` to `EventsRequest` with `calendar_id` and multiple events
- SaveEvents should save `{calendar_id}.ics` file to S3
- Remove `GetiCalendar` endpoint (not implementing)
- Replace greet.proto with proper ical2s3.proto

## Dependencies

- .NET 10.0
- Grpc.AspNetCore 2.71.0
- MinIO (for local S3 development)
- OpenTelemetry packages for instrumentation and export

## Testing

```bash
# restore, build, format and test
task ci
```
