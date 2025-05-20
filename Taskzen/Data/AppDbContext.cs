using Microsoft.EntityFrameworkCore;
using Taskzen.Entities;

namespace Taskzen.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Schedule> Schedules { get; set; } 
    public DbSet<Appointment> Appointments { get; set; } 
    public DbSet<Leave> Leaves { get; set; } 

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(entity =>
        {
            entity.HasKey(o => o.Id);
            
            entity.HasIndex(u => u.Email)
                .IsUnique();
        });
        
        builder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(s => s.CreatedByUser)
                .WithMany()
                .HasForeignKey(s => s.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.ModifiedByUser)
                .WithMany()
                .HasForeignKey(s => s.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(s => s.CreatedByUser)
                .WithMany(u => u.CreatedAppointments)
                .HasForeignKey(s => s.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.ModifiedByUser)
                .WithMany(u => u.ModifiedAppointments)
                .HasForeignKey(s => s.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        builder.Entity<Leave>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(s => s.CreatedByUser)
                .WithMany(u => u.CreatedLeaves)
                .HasForeignKey(s => s.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(s => s.ModifiedByUser)
                .WithMany(u => u.ModifiedLeaves)
                .HasForeignKey(s => s.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            var createdAtProperty = entry.Metadata.FindProperty("CreatedAt");
            if (createdAtProperty != null)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            }

        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}