# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is an incomplete gRPC service that converts events to iCalendar format and saves them to S3. It follows a hybrid language approach with C# for external interfaces and I/O operations, and F# for pure functional core logic (though F# components are not yet implemented).

## Build and Run Commands

```bash
# Run the application locally
dotnet run

# Build the project
dotnet build

# Run with Docker Compose (includes MinIO S3)
task up
```

## Architecture

The project uses:
- **C#** for gRPC services, S3 operations, and I/O
- **F#** (planned) for pure functional core logic like event processing and iCalendar generation
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

Currently no tests exist. When adding tests, use `dotnet test` to run them.
