using System;

namespace FlightDay.Api.Data;

public class TimeSlot
{
    public Guid Id { get; set; }
    public Guid FlightDayId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSlotStatus Status { get; set; } = TimeSlotStatus.Available;
    public string? BookedByUserId { get; set; }
    public string? BookedByUserName { get; set; }
    public DateTime? BookedAt { get; set; }
    public string? Notes { get; set; }
    
    // Navigation property
    public FlightDay FlightDay { get; set; } = null!;
}
