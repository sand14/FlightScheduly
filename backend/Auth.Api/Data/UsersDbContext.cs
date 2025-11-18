using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Data;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<UserRole> UserRoles { get; set; }
    
    /// <summary>
    /// Configures the EF Core model for identity-related entities and seeds initial role data.
    /// </summary>
    /// <remarks>
    /// Configures the join entity for user-role mapping with a composite primary key and required foreign keys,
    /// enforces length and required constraints for ApplicationUser first and last names, adds a unique index on Email,
    /// seeds predefined ApplicationRole entries, and sets the default schema to "identity".
    /// </remarks>
    /// <param name="builder">The ModelBuilder used to configure entity mappings and seed data.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.Property(ur => ur.AssignedAt).HasDefaultValueSql("now() at time zone 'utc'");
            
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

        // Seed default roles with static Guids (V7 format for time-ordering)
        // These are pre-generated V7 GUIDs that will remain static across migrations
        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = "018e8c5d-4f6a-7000-8000-000000000001", // V7-style GUID
                Name = Constants. Roles.Administrator,
                NormalizedName = "ADMINISTRATOR",
                Description = "Full system access",
                ConcurrencyStamp = "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"
            },
            new ApplicationRole
            {
                Id = "018e8c5d-4f6a-7000-8000-000000000002", // V7-style GUID
                Name = Constants.Roles.Instructor,
                NormalizedName = "INSTRUCTOR",
                Description = "Can create flight days and manage bookings",
                ConcurrencyStamp = "b2c3d4e5-f6a7-4b5c-9d0e-1f2a3b4c5d6e"
            },
            new ApplicationRole
            {
                Id = "018e8c5d-4f6a-7000-8000-000000000003", // V7-style GUID
                Name = Constants.Roles.Student,
                NormalizedName = "STUDENT",
                Description = "Can book flight slots",
                ConcurrencyStamp = "c3d4e5f6-a7b8-4c5d-0e1f-2a3b4c5d6e7f"
            },
            new ApplicationRole
            {
                Id = "018e8c5d-4f6a-7000-8000-000000000004", // V7-style GUID
                Name = Constants.Roles.Pilot,
                NormalizedName = "PILOT",
                Description = "Can book flight slots",
                ConcurrencyStamp = "d4e5f6a7-b8c9-4d5e-1f2a-3b4c5d6e7f8a"
            }
        );

        builder.HasDefaultSchema("identity");
    }
}