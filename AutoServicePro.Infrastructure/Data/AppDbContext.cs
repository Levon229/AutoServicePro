using AutoServicePro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoServicePro.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderPart> WorkOrderParts => Set<WorkOrderPart>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 
        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.OwnerName)
            .HasColumnName("OwnerName");

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.OwnerPhone)
            .HasColumnName("OwnerPhone");

        modelBuilder.Entity<WorkOrderPart>()
            .HasOne(workOrderPart => workOrderPart.WorkOrder)
            .WithMany(workOrder => workOrder.UsedParts)
            .HasForeignKey(workOrderPart => workOrderPart.WorkOrderId);

        modelBuilder.Entity<WorkOrderPart>()
            .HasOne(workOrderPart => workOrderPart.SparePart)
            .WithMany()
            .HasForeignKey(workOrderPart => workOrderPart.SparePartId);
    }
}