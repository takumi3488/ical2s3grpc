using System.Text;
using Grpc.Core;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace ical2s3grpc.Services;

public class ICalendarServiceImpl : ICalendarService.ICalendarServiceBase
{
    private readonly ILogger<ICalendarServiceImpl> _logger;

    public ICalendarServiceImpl(ILogger<ICalendarServiceImpl> logger)
    {
        _logger = logger;
    }

    public override Task<SaveEventsResponse> SaveEvents(EventsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("SaveEvents called for calendar_id: {CalendarId} with {EventCount} events",
            request.CalendarId, request.Events.Count);

        try
        {
            var calendar = new Calendar();

            // Set calendar-level properties from the request
            if (!string.IsNullOrEmpty(request.Prodid))
            {
                calendar.ProductId = request.Prodid;
            }
            if (!string.IsNullOrEmpty(request.Version))
            {
                calendar.Version = request.Version;
            }
            if (!string.IsNullOrEmpty(request.Calscale))
            {
                calendar.Scale = request.Calscale;
            }
            if (!string.IsNullOrEmpty(request.Method))
            {
                calendar.Method = request.Method;
            }
            if (!string.IsNullOrEmpty(request.Name))
            {
                calendar.AddProperty("X-WR-CALNAME", request.Name);
            }
            if (!string.IsNullOrEmpty(request.Description))
            {
                calendar.AddProperty("X-WR-CALDESC", request.Description);
            }
            if (!string.IsNullOrEmpty(request.Timezone))
            {
                // Add both X-WR-TIMEZONE property and VTIMEZONE component for full compatibility
                calendar.AddProperty("X-WR-TIMEZONE", request.Timezone);

                // Create a proper VTIMEZONE component
                var timezone = new VTimeZone(request.Timezone);
                calendar.TimeZones.Add(timezone);
            }

            foreach (var e in request.Events)
            {
                var calendarEvent = new CalendarEvent
                {
                    Uid = e.Uid,
                    DtStamp = new CalDateTime(DateTime.Parse(e.Dtstamp))
                };

                // Handle all-day events
                if (e.IsAllDay)
                {
                    // For all-day events, parse as date only (no time component)
                    var startDate = DateTime.Parse(e.Dtstart).Date;
                    // Use constructor overload that creates a date-only value
                    calendarEvent.DtStart = new CalDateTime(startDate.Year, startDate.Month, startDate.Day);

                    // Handle optional DTEND for all-day events
                    if (!string.IsNullOrEmpty(e.Dtend))
                    {
                        var endDate = DateTime.Parse(e.Dtend).Date;
                        calendarEvent.DtEnd = new CalDateTime(endDate.Year, endDate.Month, endDate.Day);
                    }
                }
                else
                {
                    // Set required DTSTART with timezone if specified
                    if (!string.IsNullOrEmpty(request.Timezone))
                    {
                        calendarEvent.DtStart = new CalDateTime(DateTime.Parse(e.Dtstart), request.Timezone);
                    }
                    else
                    {
                        calendarEvent.DtStart = new CalDateTime(DateTime.Parse(e.Dtstart));
                    }

                    // Handle optional DTEND
                    if (!string.IsNullOrEmpty(e.Dtend))
                    {
                        if (!string.IsNullOrEmpty(request.Timezone))
                        {
                            calendarEvent.DtEnd = new CalDateTime(DateTime.Parse(e.Dtend), request.Timezone);
                        }
                        else
                        {
                            calendarEvent.DtEnd = new CalDateTime(DateTime.Parse(e.Dtend));
                        }
                    }
                }

                // Set optional string properties
                if (!string.IsNullOrEmpty(e.Summary)) calendarEvent.Summary = e.Summary;
                if (!string.IsNullOrEmpty(e.Description)) calendarEvent.Description = e.Description;
                if (!string.IsNullOrEmpty(e.Location)) calendarEvent.Location = e.Location;
                if (!string.IsNullOrEmpty(e.Status)) calendarEvent.Status = e.Status;
                if (!string.IsNullOrEmpty(e.Class)) calendarEvent.Class = e.Class;
                if (!string.IsNullOrEmpty(e.Transp)) calendarEvent.Transparency = e.Transp;

                // Set optional organizer
                if (!string.IsNullOrEmpty(e.Organizer))
                {
                    calendarEvent.Organizer = new Organizer { CommonName = e.Organizer };
                }

                // Set optional integer properties
                if (e.HasPriority) calendarEvent.Priority = e.Priority;
                if (e.HasSequence) calendarEvent.Sequence = e.Sequence;

                // Set optional URL
                if (!string.IsNullOrEmpty(e.Url))
                {
                    calendarEvent.Url = new Uri(e.Url);
                }

                // Set optional date properties
                if (!string.IsNullOrEmpty(e.Created))
                {
                    calendarEvent.Created = new CalDateTime(DateTime.Parse(e.Created));
                }
                if (!string.IsNullOrEmpty(e.LastModified))
                {
                    calendarEvent.LastModified = new CalDateTime(DateTime.Parse(e.LastModified));
                }

                // Set related-to property
                if (!string.IsNullOrEmpty(e.RelatedTo))
                {
                    calendarEvent.AddProperty("RELATED-TO", e.RelatedTo);
                }
                // Handle recurrence rule
                if (!string.IsNullOrEmpty(e.Rrule))
                {
                    calendarEvent.RecurrenceRules.Add(new RecurrencePattern(e.Rrule));
                }

                // Handle attendees
                foreach (var attendee in e.Attendee)
                {
                    calendarEvent.Attendees.Add(new Attendee { CommonName = attendee });
                }

                // Handle categories
                foreach (var category in e.Categories)
                {
                    calendarEvent.Categories.Add(category);
                }

                // Handle comments
                foreach (var comment in e.Comment)
                {
                    calendarEvent.AddProperty("COMMENT", comment);
                }

                // Handle contacts
                foreach (var contact in e.Contact)
                {
                    calendarEvent.AddProperty("CONTACT", contact);
                }

                // Handle attachments
                foreach (var attach in e.Attach)
                {
                    calendarEvent.AddProperty("ATTACH", attach);
                }

                // Handle exception dates
                foreach (var exdate in e.Exdate)
                {
                    calendarEvent.AddProperty("EXDATE", exdate);
                }

                // Handle recurrence dates
                foreach (var rdate in e.Rdate)
                {
                    calendarEvent.AddProperty("RDATE", rdate);
                }

                // Handle resources
                foreach (var resource in e.Resources)
                {
                    calendarEvent.Resources.Add(resource);
                }

                // Handle custom properties
                foreach (var kvp in e.CustomProperties)
                {
                    calendarEvent.AddProperty(kvp.Key, kvp.Value);
                }
                calendar.Events.Add(calendarEvent);
            }

            var serializer = new CalendarSerializer();
            var icalString = serializer.SerializeToString(calendar);

            _logger.LogInformation("Generated iCalendar data:\n{ICalendarData}", icalString);

            // TODO: Implement S3 upload
            // For now, return success
            return Task.FromResult(new SaveEventsResponse
            {
                Success = true,
                ErrorMessage = ""
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving events for calendar {CalendarId}", request.CalendarId);
            return Task.FromResult(new SaveEventsResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }
}
