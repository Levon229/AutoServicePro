using AutoServicePro.Domain.Entities;

namespace AutoServicePro.Application.Interfaces;

public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    Task<WorkOrder?> GetByIdWithVehicleAsync(int id);
    Task<IReadOnlyList<WorkOrder>> GetAllWithVehicleAsync();
}