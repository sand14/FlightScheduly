using System;

namespace FlightDay.Api.Data;

public class FlightDay
{
    public Guid Id { get; set; }
    public string InstructorId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 60; // Default 1 hour slots
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FlightDayStatus Status { get; set; } = FlightDayStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<AuthorizedUser> AuthorizedUsers { get; set; } = new List<AuthorizedUser>();
    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}
