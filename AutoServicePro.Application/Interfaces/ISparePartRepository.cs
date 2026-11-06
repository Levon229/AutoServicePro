using AutoServicePro.Domain.Entities;

namespace AutoServicePro.Application.Interfaces;

public interface ISparePartRepository : IRepository<SparePart>
{
    Task<SparePart?> GetForWorkOrderAsync(int id);
    Task<bool> TryDecrementStockAsync(int id, int quantity);
}