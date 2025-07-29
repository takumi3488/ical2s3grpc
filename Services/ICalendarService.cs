using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ical2s3grpc.Infrastructure.Storage;
using ical2s3grpc.Models;
using System.Diagnostics;

namespace ical2s3grpc.Services;

public class ICalendarServiceImpl : ICalendarService.ICalendarServiceBase
{
    private readonly ILogger<ICalendarServiceImpl> _logger;
    private readonly ICalendarGenerator _calendarGenerator;
    private readonly IStorageService _storageService;
    private static readonly ActivitySource ActivitySource = new("ical2s3grpc");

    public ICalendarServiceImpl(
        ILogger<ICalendarServiceImpl> logger,
        ICalendarGenerator calendarGenerator,
        IStorageService storageService)
    {
        _logger = logger;
        _calendarGenerator = calendarGenerator;
        _storageService = storageService;
    }

    public override async Task<Empty> SaveEvents(EventsRequest request, ServerCallContext context)
    {
        using var activity = ActivitySource.StartActivity("SaveEvents");
        activity?.SetTag("calendar.id", request.CalendarId);
        activity?.SetTag("events.count", request.Events.Count);

        _logger.LogInformation("SaveEvents called for calendar_id: {CalendarId} with {EventCount} events",
            request.CalendarId, request.Events.Count);

        try
        {
            // Map request to DTO
            CalendarDto calendarDto;
            using (var mapActivity = ActivitySource.StartActivity("MapEventRequest"))
            {
                calendarDto = EventMapper.MapToCalendarDto(request);
            }

            // Generate iCalendar content
            string icalContent;
            using (var generateActivity = ActivitySource.StartActivity("GenerateICalendar"))
            {
                icalContent = _calendarGenerator.GenerateICalendar(calendarDto);
                generateActivity?.SetTag("ical.content.length", icalContent.Length);
            }

            _logger.LogDebug("Generated iCalendar data for calendar_id: {CalendarId}", request.CalendarId);

            // Save to S3 with filename {calendar_id}.ics
            var fileName = $"{request.CalendarId}.ics";
            bool success;
            using (var saveActivity = ActivitySource.StartActivity("SaveToStorage"))
            {
                saveActivity?.SetTag("storage.filename", fileName);
                success = await _storageService.SaveFileAsync(fileName, icalContent, context.CancellationToken);
                saveActivity?.SetTag("storage.success", success);
            }

            if (success)
            {
                _logger.LogInformation("Successfully saved calendar {CalendarId} to storage as {FileName}",
                    request.CalendarId, fileName);

                activity?.SetStatus(ActivityStatusCode.Ok);
                return new Empty();
            }
            else
            {
                _logger.LogError("Failed to save calendar {CalendarId} to storage", request.CalendarId);
                activity?.SetStatus(ActivityStatusCode.Error, "Failed to save calendar to storage");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to save calendar to storage"));
            }
        }
        catch (RpcException)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            // Re-throw RpcException as is
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error saving events for calendar {CalendarId}", request.CalendarId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
