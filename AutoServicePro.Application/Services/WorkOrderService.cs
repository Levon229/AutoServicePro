using System.ComponentModel.DataAnnotations;
using AutoServicePro.Application.DTOs;
using AutoServicePro.Application.Exceptions;
using AutoServicePro.Application.Interfaces;
using AutoServicePro.Domain.Entities;
using AutoServicePro.Domain.Enums;

namespace AutoServicePro.Application.Services;

public sealed class WorkOrderService(
    IWorkOrderRepository workOrderRepo,
    IRepository<Vehicle> vehicleRepo,
    ISparePartRepository sparePartRepo,
    ITransactionManager transactionManager) : IWorkOrderService
{
    public async Task<WorkOrderResponseDto> CreateWorkOrderAsync(CreateWorkOrderDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateCreateRequest(dto);

        return await transactionManager.ExecuteInTransactionAsync(async () =>
        {
            var vehicle = await vehicleRepo.GetByIdAsync(dto.VehicleId)
                ?? throw new KeyNotFoundException($"Vehicle with ID {dto.VehicleId} was not found.");

            var usedParts = new List<WorkOrderPart>();
            decimal partsTotalCost = 0;

            var requestedParts = dto.PartsToUse
                .GroupBy(part => part.SparePartId)
                .Select(group => new
                {
                    SparePartId = group.Key,
                    Quantity = group.Sum(part => (long)part.Quantity)
                })
                .ToList();

            foreach (var request in requestedParts)
            {
                if (request.Quantity > int.MaxValue)
                    throw new ValidationException($"Requested quantity for spare part {request.SparePartId} is too large.");

                var quantity = (int)request.Quantity;
                var part = await sparePartRepo.GetForWorkOrderAsync(request.SparePartId)
                    ?? throw new KeyNotFoundException($"Spare part with ID {request.SparePartId} was not found.");

                if (part.UnitPrice < 0)
                    throw new ValidationException($"Spare part '{part.Name}' has an invalid price.");

                if (part.StockQuantity < quantity)
                    throw new ConflictException(
                        $"Insufficient stock for '{part.Name}'. Available: {part.StockQuantity}, requested: {quantity}.");

                var stockUpdated = await sparePartRepo.TryDecrementStockAsync(part.Id, quantity);
                if (!stockUpdated)
                    throw new ConflictException(
                        $"Stock for '{part.Name}' changed while the work order was being created. Please retry.");

                usedParts.Add(new WorkOrderPart
                {
                    SparePartId = part.Id,
                    Quantity = quantity,
                    UnitPriceAtTime = part.UnitPrice
                });

                partsTotalCost += part.UnitPrice * quantity;
            }

            var workOrder = new WorkOrder
            {
                VehicleId = vehicle.Id,
                Vehicle = vehicle,
                IssueDescription = dto.IssueDescription.Trim(),
                LaborCost = dto.LaborCost,
                TotalCost = dto.LaborCost + partsTotalCost,
                Status = WorkOrderStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
                UsedParts = usedParts
            };

            await workOrderRepo.AddAsync(workOrder);
            await workOrderRepo.SaveChangesAsync();

            return ToResponse(workOrder);
        });
    }

    public async Task<WorkOrderResponseDto> GetWorkOrderByIdAsync(int workOrderId)
    {
        var order = await workOrderRepo.GetByIdWithVehicleAsync(workOrderId)
            ?? throw new KeyNotFoundException($"Work order with ID {workOrderId} was not found.");

        return ToResponse(order);
    }

    public async Task<IEnumerable<WorkOrderResponseDto>> GetAllWorkOrdersAsync()
    {
        var orders = await workOrderRepo.GetAllWithVehicleAsync();
        return orders.Select(ToResponse);
    }

    public async Task<WorkOrderResponseDto> CompleteWorkOrderAsync(int workOrderId)
    {
        var order = await workOrderRepo.GetByIdWithVehicleAsync(workOrderId)
            ?? throw new KeyNotFoundException($"Work order with ID {workOrderId} was not found.");

        if (order.Status == WorkOrderStatus.Completed)
            return ToResponse(order);

        if (order.Status != WorkOrderStatus.InProgress)
            throw new ConflictException($"A work order with status '{order.Status}' cannot be completed.");

        order.Status = WorkOrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;

        await workOrderRepo.SaveChangesAsync();

        return ToResponse(order);
    }

    private static void ValidateCreateRequest(CreateWorkOrderDto dto)
    {
        if (dto.VehicleId <= 0)
            throw new ValidationException("VehicleId must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.IssueDescription))
            throw new ValidationException("IssueDescription is required.");

        if (dto.IssueDescription.Length > 1000)
            throw new ValidationException("IssueDescription cannot exceed 1000 characters.");

        if (dto.LaborCost < 0 || dto.LaborCost > 999999999m)
            throw new ValidationException("LaborCost cannot be negative.");

        if (dto.PartsToUse is null)
            throw new ValidationException("PartsToUse is required.");

        if (dto.PartsToUse.Any(part => part is null || part.SparePartId <= 0 || part.Quantity <= 0))
            throw new ValidationException("Each spare part must have a valid ID and a quantity greater than zero.");
    }

    private static WorkOrderResponseDto ToResponse(WorkOrder order)
    {
        var vehicleInfo = order.Vehicle is null
            ? $"Vehicle #{order.VehicleId}"
            : $"{order.Vehicle.Year} {order.Vehicle.Make} {order.Vehicle.Model} ({order.Vehicle.LicensePlate})";

        return new WorkOrderResponseDto(
            order.Id,
            order.VehicleId,
            vehicleInfo,
            order.IssueDescription,
            order.LaborCost,
            order.TotalCost,
            order.Status.ToString(),
            order.CreatedAt);
    }
}