namespace Auth.Api.Data;

public class UserRole
{
    public required string UserId { get; set; } 
    public ApplicationUser User { get; set; } = null!;
    
    public required string RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; }
}