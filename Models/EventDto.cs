namespace ical2s3grpc.Models;

public class EventDto
{
    public string Uid { get; set; } = string.Empty;
    public DateTime DtStamp { get; set; }
    public DateTime DtStart { get; set; }
    public DateTime? DtEnd { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? Organizer { get; set; }
    public List<string> Attendees { get; set; } = new();
    public string? Status { get; set; }
    public string? Transparency { get; set; }
    public int? Sequence { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? LastModified { get; set; }
    public string? Class { get; set; }
    public int? Priority { get; set; }
    public string? Url { get; set; }
    public string? RRule { get; set; }
    public List<string> Attachments { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<string> Comments { get; set; } = new();
    public List<string> Contacts { get; set; } = new();
    public List<string> ExceptionDates { get; set; } = new();
    public List<string> RecurrenceDates { get; set; } = new();
    public string? RelatedTo { get; set; }
    public List<string> Resources { get; set; } = new();
    public Dictionary<string, string> CustomProperties { get; set; } = new();
    public bool IsAllDay { get; set; }
}

public class CalendarDto
{
    public string CalendarId { get; set; } = string.Empty;
    public List<EventDto> Events { get; set; } = new();
    public string ProductId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? CalScale { get; set; }
    public string? Method { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? TimeZone { get; set; }
}
