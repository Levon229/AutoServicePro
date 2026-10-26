using AutoServicePro.Application.DTOs;
using AutoServicePro.Application.Interfaces;
using AutoServicePro.Domain.Entities;
using AutoServicePro.Domain.Enums;

namespace AutoServicePro.Application.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<Vehicle> _vehicleRepo;
    private readonly IRepository<SparePart> _sparePartRepo;

    // Constructor Injection (DI)
    public WorkOrderService(
        IRepository<WorkOrder> workOrderRepo,
        IRepository<Vehicle> vehicleRepo,
        IRepository<SparePart> sparePartRepo)
    {
        _workOrderRepo = workOrderRepo;
        _vehicleRepo = vehicleRepo;
        _sparePartRepo = sparePartRepo;
    }

    public async Task<WorkOrderResponseDto> CreateWorkOrderAsync(CreateWorkOrderDto dto)
    {

        var vehicle = await _vehicleRepo.GetByIdAsync(dto.VehicleId);
        if (vehicle == null)
            throw new KeyNotFoundException($"Vehicle with ID {dto.VehicleId} was not found.");

        decimal partsTotalCost = 0;
        var usedParts = new List<WorkOrderPart>();


        foreach (var partReq in dto.PartsToUse)
        {
            if (partReq.Quantity <= 0)
                throw new ArgumentException("Part quantity must be greater than zero.");

            var part = await _sparePartRepo.GetByIdAsync(partReq.SparePartId);
            if (part == null)
                throw new KeyNotFoundException($"Spare part with ID {partReq.SparePartId} was not found.");

            if (part.StockQuantity < partReq.Quantity)
                throw new InvalidOperationException($"Insufficient stock for '{part.Name}'. Available: {part.StockQuantity}, Requested: {partReq.Quantity}.");


            part.StockQuantity -= partReq.Quantity;
            _sparePartRepo.Update(part);


            usedParts.Add(new WorkOrderPart
            {
                SparePartId = part.Id,
                Quantity = partReq.Quantity,
                UnitPriceAtTime = part.UnitPrice
            });

            partsTotalCost += part.UnitPrice * partReq.Quantity;
        }


        var workOrder = new WorkOrder
        {
            VehicleId = vehicle.Id,
            IssueDescription = dto.IssueDescription,
            LaborCost = dto.LaborCost,
            TotalCost = dto.LaborCost + partsTotalCost,
            Status = WorkOrderStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            UsedParts = usedParts
        };

        await _workOrderRepo.AddAsync(workOrder);
        await _workOrderRepo.SaveChangesAsync();

        return new WorkOrderResponseDto(
            workOrder.Id,
            vehicle.Id,
            $"{vehicle.Year} {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate})",
            workOrder.IssueDescription,
            workOrder.LaborCost,
            workOrder.TotalCost,
            workOrder.Status.ToString(),
            workOrder.CreatedAt);
    }

    public async Task<IEnumerable<WorkOrderResponseDto>> GetAllWorkOrdersAsync()
    {
        var orders = await _workOrderRepo.GetAllAsync();
        return orders.Select(o => new WorkOrderResponseDto(
            o.Id,
            o.VehicleId,
            o.Vehicle != null ? $"{o.Vehicle.Make} {o.Vehicle.Model}" : $"Vehicle #{o.VehicleId}",
            o.IssueDescription,
            o.LaborCost,
            o.TotalCost,
            o.Status.ToString(),
            o.CreatedAt));
    }

    public async Task<WorkOrderResponseDto> CompleteWorkOrderAsync(int workOrderId)
    {
        var order = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (order == null)
            throw new KeyNotFoundException($"Work order with ID {workOrderId} was not found.");

        order.Status = WorkOrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;

        _workOrderRepo.Update(order);
        await _workOrderRepo.SaveChangesAsync();

        return new WorkOrderResponseDto(
            order.Id,
            order.VehicleId,
            $"Vehicle #{order.VehicleId}",
            order.IssueDescription,
            order.LaborCost,
            order.TotalCost,
            order.Status.ToString(),
            order.CreatedAt);
    }
}