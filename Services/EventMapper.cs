using System.Globalization;
using ical2s3grpc.Models;

namespace ical2s3grpc.Services;

public static class EventMapper
{
    public static CalendarDto MapToCalendarDto(EventsRequest request)
    {
        return new CalendarDto
        {
            CalendarId = request.CalendarId,
            ProductId = request.Prodid,
            Version = request.Version,
            CalScale = request.Calscale,
            Method = request.Method,
            Name = request.Name,
            Description = request.Description,
            TimeZone = request.Timezone,
            Events = request.Events.Select(MapToEventDto).ToList()
        };
    }

    public static EventDto MapToEventDto(Event protoEvent)
    {
        return new EventDto
        {
            Uid = protoEvent.Uid,
            DtStamp = ParseICalDateTime(protoEvent.Dtstamp),
            DtStart = ParseICalDateTime(protoEvent.Dtstart),
            DtEnd = !string.IsNullOrEmpty(protoEvent.Dtend) ? ParseICalDateTime(protoEvent.Dtend) : null,
            Summary = protoEvent.Summary,
            Description = protoEvent.Description,
            Location = protoEvent.Location,
            Organizer = protoEvent.Organizer,
            Attendees = protoEvent.Attendee.ToList(),
            Status = protoEvent.Status,
            Transparency = protoEvent.Transp,
            Sequence = protoEvent.HasSequence ? protoEvent.Sequence : null,
            Created = !string.IsNullOrEmpty(protoEvent.Created) ? ParseICalDateTime(protoEvent.Created) : null,
            LastModified = !string.IsNullOrEmpty(protoEvent.LastModified) ? ParseICalDateTime(protoEvent.LastModified) : null,
            Class = protoEvent.Class,
            Priority = protoEvent.HasPriority ? protoEvent.Priority : null,
            Url = protoEvent.Url,
            RRule = protoEvent.Rrule,
            Attachments = protoEvent.Attach.ToList(),
            Categories = protoEvent.Categories.ToList(),
            Comments = protoEvent.Comment.ToList(),
            Contacts = protoEvent.Contact.ToList(),
            ExceptionDates = protoEvent.Exdate.ToList(),
            RecurrenceDates = protoEvent.Rdate.ToList(),
            RelatedTo = protoEvent.RelatedTo,
            Resources = protoEvent.Resources.ToList(),
            CustomProperties = protoEvent.CustomProperties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            IsAllDay = protoEvent.IsAllDay
        };
    }

    private static DateTime ParseICalDateTime(string dateTimeString)
    {
        // Common iCalendar date-time formats
        string[] formats = new[]
        {
            "yyyyMMddTHHmmssZ",     // UTC format: 20250129T120000Z
            "yyyyMMddTHHmmss",      // Local format: 20250129T120000
            "yyyyMMdd",             // Date only: 20250129
            "yyyy-MM-ddTHH:mm:ssZ", // ISO 8601 with separators
            "yyyy-MM-ddTHH:mm:ss"   // ISO 8601 local with separators
        };

        if (DateTime.TryParseExact(dateTimeString, formats, CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime result))
        {
            return result;
        }

        // Fallback to regular parse for other formats
        return DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture);
    }
}
