using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Data;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<UserRole> UserRoles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });
        
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
        });
        
        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole 
            { 
                Id = "d290f1ee-6c54-4b01-90e6-d701748f0851", 
                Name = Constants.Roles.Administrator, 
                NormalizedName = "ADMINISTRATOR",
                Description = "Full system access",
                ConcurrencyStamp = "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"
            },
            new ApplicationRole 
            { 
                Id = "d290f1ee-6c54-4b01-90e6-d701748f0852", 
                Name = Constants.Roles.Instructor, 
                NormalizedName = "INSTRUCTOR",
                Description = "Can create flight days and manage bookings",
                ConcurrencyStamp = "b2c3d4e5-f6a7-4b5c-9d0e-1f2a3b4c5d6e"
            },
            new ApplicationRole 
            { 
                Id = "d290f1ee-6c54-4b01-90e6-d701748f0853", 
                Name = Constants.Roles.Student, 
                NormalizedName = "STUDENT",
                Description = "Can book flight slots",
                ConcurrencyStamp = "c3d4e5f6-a7b8-4c5d-0e1f-2a3b4c5d6e7f"
            },
            new ApplicationRole 
            { 
                Id = "d290f1ee-6c54-4b01-90e6-d701748f0854", 
                Name = Constants.Roles.Pilot, 
                NormalizedName = "PILOT",
                Description = "Can book flight slots",
                ConcurrencyStamp = "d4e5f6a7-b8c9-4d5e-1f2a-3b4c5d6e7f8a"
            }
        );

        builder.HasDefaultSchema("identity");
    }
}