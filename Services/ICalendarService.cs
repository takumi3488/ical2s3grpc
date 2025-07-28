using Grpc.Core;

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
            // TODO: Implement iCalendar generation and S3 upload
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
