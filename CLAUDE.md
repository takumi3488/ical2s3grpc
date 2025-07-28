# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is an incomplete gRPC service that converts events to iCalendar format and saves them to S3. It is built with C# for all components.

## Build and Run Commands

### Environment Variables
All S3 configuration is now done via environment variables:

```bash
# Required environment variables
export S3_ACCESS_KEY="your-access-key"
export S3_SECRET_KEY="your-secret-key"
export S3_BUCKET_NAME="your-bucket-name"

# Optional environment variables (with AWS S3 defaults)
export S3_SERVICE_URL=""                          # empty for AWS S3 (auto-detected), required for MinIO/custom endpoints
export S3_REGION="us-east-1"                      # defaults to us-east-1
export S3_USE_HTTPS="true"                        # defaults to true
export S3_FORCE_PATH_STYLE="false"                # defaults to false (AWS S3 style)

# Run the application locally
dotnet run

# Build the project
dotnet build

# For development with MinIO (use docker compose)
# Note: MinIO requires S3_FORCE_PATH_STYLE=true and S3_USE_HTTPS=false
docker compose up
```

**Note for MinIO users**: MinIO requires different settings than AWS S3:
- `S3_SERVICE_URL="http://localhost:9000"` (or your MinIO URL)
- `S3_FORCE_PATH_STYLE="true"` (required for MinIO)
- `S3_USE_HTTPS="false"` (if using HTTP)

## Architecture

The project uses:
- **C#** for all components including gRPC services, S3 operations, and business logic
- **gRPC** for service interface (currently using greet.proto but should be replaced with ical2s3.proto)
- **MinIO** as local S3 storage in development

## Current State and TODO

The project is incomplete. Based on instruction.md, the following changes need to be made:
- Rename `ConvertToiCalendar` to `SaveEvents`
- Rename `EventRequest` to `EventsRequest` with `calendar_id` and multiple events
- SaveEvents should save `{calendar_id}.ics` file to S3
- Remove `GetiCalendar` endpoint (not implementing)
- Replace greet.proto with proper ical2s3.proto

## Dependencies

- .NET 9.0
- Grpc.AspNetCore 2.71.0
- MinIO (for local S3 development)

## Testing

```bash
# restore, build, format and test
task ci
```
