# ical2s3grpc

🚧 **This project is incomplete** 🚧

This project is currently under development and is not yet complete. 

## Configuration

### Environment Variables

The application can be configured using the following environment variables:

#### S3 Configuration (Required)
| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `S3_ACCESS_KEY` | S3 access key | - | `your-access-key` |
| `S3_SECRET_KEY` | S3 secret key | - | `your-secret-key` |
| `S3_BUCKET_NAME` | S3 bucket name | - | `ical2s3grpc` |

#### S3 Configuration (Optional)
| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `S3_SERVICE_URL` | S3 service endpoint URL | Empty (AWS S3 auto-detected) | `http://localhost:9000` |
| `S3_REGION` | AWS region | `us-east-1` | `us-west-2` |
| `S3_USE_HTTPS` | Use HTTPS for S3 connections | `true` | `false` |
| `S3_FORCE_PATH_STYLE` | Force path-style addressing | `false` | `true` |

#### OpenTelemetry Configuration (Optional)
| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OTLP endpoint for tracing | Empty | `http://localhost:4317` |
| `OTEL_SERVICE_NAME` | Service name for tracing | `ical2s3grpc` | `my-service` |

### Configuration Examples

#### AWS S3 Configuration
```bash
export S3_ACCESS_KEY="your-aws-access-key"
export S3_SECRET_KEY="your-aws-secret-key"
export S3_BUCKET_NAME="my-ical-bucket"
export S3_REGION="us-west-2"
```

#### MinIO Configuration
```bash
export S3_ACCESS_KEY="admin"
export S3_SECRET_KEY="minio123"
export S3_SERVICE_URL="http://localhost:9000"
export S3_BUCKET_NAME="ical2s3grpc"
export S3_REGION="us-east-1"
export S3_USE_HTTPS="false"
export S3_FORCE_PATH_STYLE="true"
```

#### With Distributed Tracing (Jaeger)
```bash
# Add to either AWS or MinIO configuration
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4317"
export OTEL_SERVICE_NAME="ical2s3grpc"
```

## Architecture

The project uses a simple, pragmatic C# architecture:
- **C#**: Used for gRPC services, iCalendar generation (via [iCal.NET](https://github.com/ical-org/ical.net)), and S3 operations
- **iCal.NET**: Industry-standard library for generating RFC 5545 compliant iCalendar files
- **AWS SDK**: For S3 bucket operations

## Directory Structure

```
ical2s3grpc/
├── Program.cs                         # Application entry point and DI configuration
├── Protos/                            # Protocol buffer definitions
│   └── ical2s3.proto                  # gRPC service contract
├── Services/                          # gRPC service implementations
│   ├── ICalendarService.cs            # Main gRPC service
│   └── CalendarGenerator.cs           # iCal.NET wrapper for calendar generation
├── Infrastructure/                    # External dependencies and I/O
│   ├── Storage/
│   │   ├── IStorageService.cs         # Storage abstraction
│   │   ├── S3StorageService.cs        # S3 client implementation
│   │   └── MockStorageService.cs      # Mock storage for testing
│   └── Configuration/
│       └── S3Options.cs               # Configuration models
├── Models/                            # Simple data models
│   └── EventDto.cs                    # DTO for mapping between proto and iCal.NET
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── ical2s3grpc.csproj
└── ical2s3grpc.sln
```

This structure follows standard .NET conventions with a focus on simplicity and maintainability. The architecture separates concerns without over-engineering:
- **Services**: Business logic and gRPC service implementations
- **Infrastructure**: External dependencies (S3, configuration)
- **Models**: Simple data transfer objects

## Dependencies

- .NET 9.0
- Grpc.AspNetCore 2.71.0
- AWSSDK.S3 3.7.413
- Ical.Net 4.3.0 (for iCalendar generation)
- MinIO (for local S3 development)

## Development

Run `dotnet run` to start the project.

## TODO

### Missing Components
- [ ] **Models/EventDto.cs** - DTO for mapping between proto and iCal.NET (currently not implemented, service directly maps from proto to iCal.NET)
- [ ] **Services/CalendarGenerator.cs** - iCal.NET wrapper for calendar generation (currently logic is embedded in ICalendarService.cs)
- [ ] **S3 Integration in SaveEvents** - The S3 upload functionality is not implemented (see TODO comment at line 218 in ICalendarService.cs)
- [ ] **Dependency Injection** - IStorageService is not injected into ICalendarService
- [ ] **Unit Tests** - No test project or unit tests implemented
- [ ] **Integration Tests** - No integration tests for S3 functionality
- [ ] **Error Handling** - More robust error handling for S3 operations
- [ ] **Logging** - Structured logging for S3 operations
- [ ] **Health Checks** - gRPC health check service implementation
- [ ] **Docker Support** - Complete Dockerfile implementation and docker-compose configuration

### Implementation Tasks
- [ ] Inject IStorageService into ICalendarService constructor
- [ ] Implement S3 upload in SaveEvents method (replace TODO at line 218)
- [ ] Create CalendarGenerator service to separate iCal.NET logic from gRPC service
- [ ] Add EventDto model for cleaner separation of concerns
- [ ] Add comprehensive error handling for S3 operations
- [ ] Implement retry logic for S3 operations
- [ ] Add configuration validation at startup
- [ ] Create unit test project with tests for CalendarGenerator
- [ ] Create integration test project with tests for S3 operations
- [ ] Add GitHub Actions CI/CD pipeline

## gRPC Endpoints

The service exposes the following gRPC endpoints:

### `SaveEvents`

Converts multiple event data into the iCalendar format and saves the resulting `.ics` file to an S3 bucket with the filename `{calendar_id}.ics`.

#### Request: `EventsRequest`

| Field             | Type                | Description                                                                                             |
| ----------------- | ------------------- | ------------------------------------------------------------------------------------------------------- |
| `calendar_id`     | `string`            | Unique identifier for the calendar. Used as the filename for the generated `.ics` file.                |
| `events`          | `repeated Event`    | A list of events to be converted and saved to the calendar.                                             |
| `prodid`          | `string`            | Product identifier for the calendar (REQUIRED - corresponds to PRODID property).                        |
| `version`         | `string`            | iCalendar version (REQUIRED - corresponds to VERSION property).                                         |
| `calscale`        | `string` (optional) | Calendar scale (defaults to GREGORIAN).                                                                 |
| `method`          | `string` (optional) | Calendar method (e.g., REQUEST, REPLY, CANCEL).                                                        |
| `name`            | `string` (optional) | Calendar name (X-WR-CALNAME).                                                                           |
| `description`     | `string` (optional) | Calendar description (X-WR-CALDESC).                                                                    |
| `timezone`        | `string` (optional) | Default timezone for the calendar (VTIMEZONE/X-WR-TIMEZONE).                                           |

##### Event Message

| Field             | Type                      | Description                                                                                             |
| ----------------- | ------------------------- | ------------------------------------------------------------------------------------------------------- |
| `uid`             | `string`                  | Unique identifier for the event (REQUIRED).                                                            |
| `dtstamp`         | `string`                  | Timestamp of when the event was created (REQUIRED).                                                    |
| `dtstart`         | `string`                  | Start time of the event (REQUIRED).                                                                    |
| `dtend`           | `string` (optional)       | End time of the event.                                                                                  |
| `summary`         | `string` (optional)       | Event title/summary.                                                                                    |
| `description`     | `string` (optional)       | Detailed event description.                                                                             |
| `location`        | `string` (optional)       | Event location.                                                                                         |
| `organizer`       | `string` (optional)       | Event organizer.                                                                                        |
| `attendee`        | `repeated string`         | List of attendees.                                                                                      |
| `status`          | `string` (optional)       | Event status (e.g., TENTATIVE, CONFIRMED, CANCELLED).                                                  |
| `transp`          | `string` (optional)       | Time transparency (OPAQUE or TRANSPARENT).                                                             |
| `sequence`        | `int32` (optional)        | Revision sequence number.                                                                               |
| `created`         | `string` (optional)       | Creation timestamp.                                                                                     |
| `last_modified`   | `string` (optional)       | Last modification timestamp.                                                                            |
| `class`           | `string` (optional)       | Classification (PUBLIC, PRIVATE, CONFIDENTIAL).                                                         |
| `priority`        | `int32` (optional)        | Priority level (0-9).                                                                                   |
| `url`             | `string` (optional)       | URL associated with the event.                                                                          |
| `rrule`           | `string` (optional)       | Recurrence rule.                                                                                        |
| `attach`          | `repeated string`         | File attachments.                                                                                       |
| `categories`      | `repeated string`         | Event categories.                                                                                       |
| `comment`         | `repeated string`         | Comments.                                                                                               |
| `contact`         | `repeated string`         | Contact information.                                                                                    |
| `exdate`          | `repeated string`         | Exception dates for recurring events.                                                                   |
| `rdate`           | `repeated string`         | Recurrence dates.                                                                                       |
| `related_to`      | `string` (optional)       | Related event UID.                                                                                      |
| `resources`       | `repeated string`         | Resources needed for the event.                                                                         |
| `custom_properties` | `map<string, string>`   | Custom key-value pairs.                                                                                 |
| `is_all_day`      | `bool` (optional)         | Indicates if this is an all-day event.                                                                 |

#### Response

The method returns `google.protobuf.Empty` on success. Errors are communicated through standard gRPC status codes:
- `OK` (0): Successfully saved the calendar to S3
- `INTERNAL` (13): Failed to save to storage or other internal errors
