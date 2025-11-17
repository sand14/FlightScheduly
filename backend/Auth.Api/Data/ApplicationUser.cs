using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Data;

public class ApplicationUser : IdentityUser
{
   public string FirstName { get; set; } = string.Empty;
   public string LastName { get; set; } = string.Empty;
   public UserType UserType { get; set; }
   public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
   public DateTime? LastLoginAt { get; set; }
   public DateTime? LicenseExpirationDate { get; set; }
   public DateTime? RadioLicenseExpirationDate { get; set; }
   public DateTime? MedicalLicenseExpirationDate { get; set; }
   public bool IsActive { get; set; } = true;
   
   // Navigation properties
   public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}