using System;

namespace FlightDay.Api.Data;

public class AuthorizedUser
{
    public Guid Id { get; set; }
    public Guid FlightDayId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime AuthorizedAt { get; set; }
    
    // Navigation property
    public FlightDay FlightDay { get; set; } = null!;
}
