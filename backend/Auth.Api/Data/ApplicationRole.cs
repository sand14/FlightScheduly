using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Data;

public class ApplicationRole : IdentityRole
{
    public string Description { get; set; } = string.Empty;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}