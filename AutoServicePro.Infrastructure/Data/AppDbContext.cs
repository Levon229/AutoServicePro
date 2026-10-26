using AutoServicePro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoServicePro.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderPart> WorkOrderParts => Set<WorkOrderPart>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        modelBuilder.Entity<WorkOrderPart>()
            .HasOne(wp => wp.WorkOrder)
            .WithMany(w => w.UsedParts)
            .HasForeignKey(wp => wp.WorkOrderId);

        modelBuilder.Entity<WorkOrderPart>()
            .HasOne(wp => wp.SparePart)
            .WithMany()
            .HasForeignKey(wp => wp.SparePartId);
    }
}