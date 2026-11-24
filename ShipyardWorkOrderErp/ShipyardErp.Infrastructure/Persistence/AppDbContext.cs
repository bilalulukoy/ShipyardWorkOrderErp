using Microsoft.EntityFrameworkCore;
using ShipyardErp.Application.Interfaces;
using ShipyardErp.Domain.Entities;

namespace ShipyardErp.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShipProject> ShipProjects => Set<ShipProject>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<MaterialRequest> MaterialRequests => Set<MaterialRequest>();
    public DbSet<ErpSyncOutbox> ErpSyncOutboxes => Set<ErpSyncOutbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShipProject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProjectCode).IsUnique();
            entity.Property(e => e.ProjectCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VesselName).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WorkOrderNumber).IsUnique();
            entity.Property(e => e.WorkOrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EstimatedManHours).HasPrecision(10, 2);

            entity.HasOne(e => e.ShipProject)
                  .WithMany(p => p.WorkOrders)
                  .HasForeignKey(e => e.ShipProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MaterialRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(12, 3);

            entity.HasOne(e => e.WorkOrder)
                  .WithMany(w => w.MaterialRequests)
                  .HasForeignKey(e => e.WorkOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ErpSyncOutbox>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Status, e.NextRetryAt });
        });
    }
}