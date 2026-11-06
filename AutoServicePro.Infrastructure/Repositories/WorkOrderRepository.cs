using AutoServicePro.Application.Interfaces;
using AutoServicePro.Domain.Entities;
using AutoServicePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoServicePro.Infrastructure.Repositories;

public sealed class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    private readonly AppDbContext _context;

    public WorkOrderRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<WorkOrder?> GetByIdWithVehicleAsync(int id)
    {
        return _context.WorkOrders
            .Include(order => order.Vehicle)
            .SingleOrDefaultAsync(order => order.Id == id);
    }

    public async Task<IReadOnlyList<WorkOrder>> GetAllWithVehicleAsync()
    {
        return await _context.WorkOrders
            .AsNoTracking()
            .Include(order => order.Vehicle)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync();
    }
}