# ical2s3grpc

🚧 **This project is incomplete** 🚧

This project is currently under development and is not yet complete. 

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

#### Response: `SaveEventsResponse`

| Field             | Type                | Description                                                                                             |
| ----------------- | ------------------- | ------------------------------------------------------------------------------------------------------- |
| `success`         | `bool`              | Indicates whether the operation was successful.                                                         |
| `error_message`   | `string`            | Error message if the operation failed.
