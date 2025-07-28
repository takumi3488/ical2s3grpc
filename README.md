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

##### Event Message

| Field             | Type                | Description                                                                                             |
| ----------------- | ------------------- | ------------------------------------------------------------------------------------------------------- |
| `event_id`        | `string`            | Unique identifier for the event.                                                                        |
| `title`           | `string`            | The title or summary of the event.                                                                      |
| `description`     | `string`            | A more detailed description of the event.                                                               |
| `start_time`      | `string`            | The start time of the event in ISO 8601 format (e.g., `2023-10-27T10:00:00Z`).                             |
| `end_time`        | `string`            | The end time of the event in ISO 8601 format.                                                           |
| `location`        | `string`            | The location where the event will take place.                                                           |
| `attendees`       | `repeated string`   | A list of attendee email addresses.                                                                     |
| `organizer`       | `string`            | The email address of the event organizer.                                                               |
| `timezone`        | `string`            | The timezone for the event (e.g., `America/New_York`).                                                  |
| `custom_properties` | `map<string, string>` | A map of custom key-value pairs to be included in the iCalendar file.                                   |

#### Response: Status Code Only

The `SaveEvents` method returns only a gRPC status code indicating success or failure of the operation.
