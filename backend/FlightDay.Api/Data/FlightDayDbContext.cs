using System;
using Microsoft.EntityFrameworkCore;

namespace FlightDay.Api.Data;

public class FlightDayDbContext : DbContext
{
    public FlightDayDbContext(DbContextOptions<FlightDayDbContext> options)
        : base(options)
    {
    }

    public DbSet<FlightDay> FlightDays { get; set; }
    public DbSet<TimeSlot> TimeSlots { get; set; }
    public DbSet<AuthorizedUser> AuthorizedUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure FlightDay
        modelBuilder.Entity<FlightDay>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.InstructorId).IsRequired().HasMaxLength(450);
            entity.Property(f => f.Location).IsRequired().HasMaxLength(200);
            entity.Property(f => f.Description).HasMaxLength(1000);
            entity.Property(f => f.Date).IsRequired();
            entity.Property(f => f.Status).IsRequired();
            entity.Property(f => f.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            
            entity.HasIndex(f => new { f.Date, f.InstructorId }).IsUnique();
            entity.HasIndex(f => f.Status);
            
            entity.HasMany(f => f.AuthorizedUsers)
                .WithOne(a => a.FlightDay)
                .HasForeignKey(a => a.FlightDayId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(f => f.TimeSlots)
                .WithOne(t => t.FlightDay)
                .HasForeignKey(t => t.FlightDayId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure AuthorizedUser
        modelBuilder.Entity<AuthorizedUser>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.UserId).IsRequired().HasMaxLength(450);
            entity.Property(a => a.UserEmail).IsRequired().HasMaxLength(256);
            entity.Property(a => a.UserName).IsRequired().HasMaxLength(256);
            entity.Property(a => a.AuthorizedAt).HasDefaultValueSql("now() at time zone 'utc'");
            
            entity.HasIndex(a => new { a.FlightDayId, a.UserId }).IsUnique();
        });

        // Configure TimeSlot
        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.StartTime).IsRequired();
            entity.Property(t => t.EndTime).IsRequired();
            entity.Property(t => t.Status).IsRequired();
            entity.Property(t => t.BookedByUserId).HasMaxLength(450);
            entity.Property(t => t.BookedByUserName).HasMaxLength(256);
            entity.Property(t => t.Notes).HasMaxLength(500);
            
            entity.HasIndex(t => new { t.FlightDayId, t.StartTime });
            entity.HasIndex(t => t.Status);
        });
    }
}
