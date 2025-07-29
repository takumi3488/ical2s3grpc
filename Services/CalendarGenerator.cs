using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using ical2s3grpc.Models;

namespace ical2s3grpc.Services;

public interface ICalendarGenerator
{
    string GenerateICalendar(CalendarDto calendarDto);
}

public class CalendarGenerator : ICalendarGenerator
{
    private readonly ILogger<CalendarGenerator> _logger;

    public CalendarGenerator(ILogger<CalendarGenerator> logger)
    {
        _logger = logger;
    }

    public string GenerateICalendar(CalendarDto calendarDto)
    {
        _logger.LogDebug("Generating iCalendar for calendar_id: {CalendarId} with {EventCount} events",
            calendarDto.CalendarId, calendarDto.Events.Count);

        var calendar = new Calendar();

        SetCalendarProperties(calendar, calendarDto);
        AddEventsToCalendar(calendar, calendarDto.Events, calendarDto.TimeZone);

        var serializer = new CalendarSerializer();
        var icalString = serializer.SerializeToString(calendar);

        _logger.LogDebug("Generated iCalendar data for calendar_id: {CalendarId}", calendarDto.CalendarId);
        return icalString;
    }

    private void SetCalendarProperties(Calendar calendar, CalendarDto calendarDto)
    {
        if (!string.IsNullOrEmpty(calendarDto.ProductId))
        {
            calendar.ProductId = calendarDto.ProductId;
        }

        if (!string.IsNullOrEmpty(calendarDto.Version))
        {
            calendar.Version = calendarDto.Version;
        }

        if (!string.IsNullOrEmpty(calendarDto.CalScale))
        {
            calendar.Scale = calendarDto.CalScale;
        }

        if (!string.IsNullOrEmpty(calendarDto.Method))
        {
            calendar.Method = calendarDto.Method;
        }

        if (!string.IsNullOrEmpty(calendarDto.Name))
        {
            calendar.AddProperty("X-WR-CALNAME", calendarDto.Name);
        }

        if (!string.IsNullOrEmpty(calendarDto.Description))
        {
            calendar.AddProperty("X-WR-CALDESC", calendarDto.Description);
        }

        if (!string.IsNullOrEmpty(calendarDto.TimeZone))
        {
            calendar.AddProperty("X-WR-TIMEZONE", calendarDto.TimeZone);
            var timezone = new VTimeZone(calendarDto.TimeZone);
            calendar.TimeZones.Add(timezone);
        }
    }

    private void AddEventsToCalendar(Calendar calendar, List<EventDto> events, string? timeZone)
    {
        foreach (var eventDto in events)
        {
            var calendarEvent = CreateCalendarEvent(eventDto, timeZone);
            calendar.Events.Add(calendarEvent);
        }
    }

    private CalendarEvent CreateCalendarEvent(EventDto eventDto, string? timeZone)
    {
        var calendarEvent = new CalendarEvent
        {
            Uid = eventDto.Uid,
            DtStamp = new CalDateTime(eventDto.DtStamp)
        };

        SetEventDateTimes(calendarEvent, eventDto, timeZone);
        SetEventProperties(calendarEvent, eventDto);
        SetEventCollections(calendarEvent, eventDto);

        return calendarEvent;
    }

    private void SetEventDateTimes(CalendarEvent calendarEvent, EventDto eventDto, string? timeZone)
    {
        if (eventDto.IsAllDay)
        {
            var startDate = eventDto.DtStart.Date;
            calendarEvent.DtStart = new CalDateTime(startDate.Year, startDate.Month, startDate.Day);

            if (eventDto.DtEnd.HasValue)
            {
                var endDate = eventDto.DtEnd.Value.Date;
                calendarEvent.DtEnd = new CalDateTime(endDate.Year, endDate.Month, endDate.Day);
            }
        }
        else
        {
            calendarEvent.DtStart = !string.IsNullOrEmpty(timeZone)
                ? new CalDateTime(eventDto.DtStart, timeZone)
                : new CalDateTime(eventDto.DtStart);

            if (eventDto.DtEnd.HasValue)
            {
                calendarEvent.DtEnd = !string.IsNullOrEmpty(timeZone)
                    ? new CalDateTime(eventDto.DtEnd.Value, timeZone)
                    : new CalDateTime(eventDto.DtEnd.Value);
            }
        }
    }

    private void SetEventProperties(CalendarEvent calendarEvent, EventDto eventDto)
    {
        if (!string.IsNullOrEmpty(eventDto.Summary)) calendarEvent.Summary = eventDto.Summary;
        if (!string.IsNullOrEmpty(eventDto.Description)) calendarEvent.Description = eventDto.Description;
        if (!string.IsNullOrEmpty(eventDto.Location)) calendarEvent.Location = eventDto.Location;
        if (!string.IsNullOrEmpty(eventDto.Status)) calendarEvent.Status = eventDto.Status;
        if (!string.IsNullOrEmpty(eventDto.Class)) calendarEvent.Class = eventDto.Class;
        if (!string.IsNullOrEmpty(eventDto.Transparency)) calendarEvent.Transparency = eventDto.Transparency;

        if (!string.IsNullOrEmpty(eventDto.Organizer))
        {
            calendarEvent.Organizer = new Organizer { CommonName = eventDto.Organizer };
        }

        if (eventDto.Priority.HasValue) calendarEvent.Priority = eventDto.Priority.Value;
        if (eventDto.Sequence.HasValue) calendarEvent.Sequence = eventDto.Sequence.Value;

        if (!string.IsNullOrEmpty(eventDto.Url))
        {
            calendarEvent.Url = new Uri(eventDto.Url);
        }

        if (eventDto.Created.HasValue)
        {
            calendarEvent.Created = new CalDateTime(eventDto.Created.Value);
        }

        if (eventDto.LastModified.HasValue)
        {
            calendarEvent.LastModified = new CalDateTime(eventDto.LastModified.Value);
        }

        if (!string.IsNullOrEmpty(eventDto.RelatedTo))
        {
            calendarEvent.AddProperty("RELATED-TO", eventDto.RelatedTo);
        }

        if (!string.IsNullOrEmpty(eventDto.RRule))
        {
            calendarEvent.RecurrenceRules.Add(new RecurrencePattern(eventDto.RRule));
        }
    }

    private void SetEventCollections(CalendarEvent calendarEvent, EventDto eventDto)
    {
        foreach (var attendee in eventDto.Attendees)
        {
            calendarEvent.Attendees.Add(new Attendee { CommonName = attendee });
        }

        foreach (var category in eventDto.Categories)
        {
            calendarEvent.Categories.Add(category);
        }

        foreach (var comment in eventDto.Comments)
        {
            calendarEvent.AddProperty("COMMENT", comment);
        }

        foreach (var contact in eventDto.Contacts)
        {
            calendarEvent.AddProperty("CONTACT", contact);
        }

        foreach (var attachment in eventDto.Attachments)
        {
            calendarEvent.AddProperty("ATTACH", attachment);
        }

        foreach (var exdate in eventDto.ExceptionDates)
        {
            calendarEvent.AddProperty("EXDATE", exdate);
        }

        foreach (var rdate in eventDto.RecurrenceDates)
        {
            calendarEvent.AddProperty("RDATE", rdate);
        }

        foreach (var resource in eventDto.Resources)
        {
            calendarEvent.Resources.Add(resource);
        }

        foreach (var kvp in eventDto.CustomProperties)
        {
            calendarEvent.AddProperty(kvp.Key, kvp.Value);
        }
    }
}
