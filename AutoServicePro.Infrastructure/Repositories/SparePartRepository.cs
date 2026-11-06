using AutoServicePro.Application.Interfaces;
using AutoServicePro.Domain.Entities;
using AutoServicePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoServicePro.Infrastructure.Repositories;

public sealed class SparePartRepository : Repository<SparePart>, ISparePartRepository
{
    private readonly AppDbContext _context;

    public SparePartRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<SparePart?> GetForWorkOrderAsync(int id)
    {
        return _context.SpareParts
            .AsNoTracking()
            .SingleOrDefaultAsync(part => part.Id == id);
    }

    public async Task<bool> TryDecrementStockAsync(int id, int quantity)
    {
        var affectedRows = await _context.SpareParts
            .Where(part => part.Id == id && part.StockQuantity >= quantity)
            .ExecuteUpdateAsync(updates => updates.SetProperty(
                part => part.StockQuantity,
                part => part.StockQuantity - quantity));

        return affectedRows == 1;
    }
}