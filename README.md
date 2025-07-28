# ical2s3grpc

🚧 **This project is incomplete** 🚧

This project is currently under development and is not yet complete. 

## Architecture

The project follows a hybrid language approach:
- **C#**: Used for external interfaces and side effects (gRPC services, S3 operations, I/O)
- **F#**: Used for pure functional core logic (event processing, iCalendar generation, data transformations)

## Directory Structure

```
ical2s3grpc/
├── Program.cs                     # C# - Application entry point and DI configuration
├── Protos/                        # Protocol buffer definitions
│   └── ical2s3.proto              # gRPC service contract
├── Services/                      # C# - gRPC service implementations
│   └── ICalendarService.cs        # Main gRPC service
├── Infrastructure/                # C# - External dependencies and I/O
│   ├── S3/
│   │   └── S3StorageService.cs    # S3 client wrapper
│   └── Configuration/
│       └── S3Options.cs           # Configuration models
├── Core/                          # F# - Pure functional core
│   ├── Domain/
│   │   ├── ValueObjects/          # Value objects for type safety
│   │   │   ├── CalendarId.fs      # Calendar identifier
│   │   │   ├── EventId.fs         # Event identifier
│   │   │   ├── EmailAddress.fs    # Email address validation
│   │   │   ├── TimeZone.fs        # Timezone wrapper
│   │   │   └── DateTimeRange.fs   # Event time range
│   │   ├── Entities/              # Domain entities
│   │   │   ├── Event.fs           # Event domain models
│   │   │   └── Calendar.fs        # Calendar domain models
│   │   └── Aggregates/            # Domain aggregates
│   │       └── EventCollection.fs # Event collection aggregate
│   ├── Services/
│   │   ├── EventProcessor.fs      # Event processing logic
│   │   └── ICalendarGenerator.fs  # iCalendar format generation
│   └── Mappers/
│       └── EventMapper.fs         # Data transformation functions
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── ical2s3grpc.csproj
└── ical2s3grpc.sln
```

This structure follows .NET conventions while maintaining clear separation between imperative I/O operations (C#) and pure functional business logic (F#). The domain layer is organized with value objects for type safety, entities for core business objects, and aggregates for maintaining consistency boundaries.

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
