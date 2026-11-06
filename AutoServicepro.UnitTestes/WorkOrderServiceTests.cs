using AutoServicePro.Application.DTOs;
using AutoServicePro.Application.Exceptions;
using AutoServicePro.Application.Interfaces;
using AutoServicePro.Application.Services;
using AutoServicePro.Domain.Entities;
using AutoServicePro.Domain.Enums;
using FluentAssertions;
using Moq;

namespace AutoServicePro.UnitTests;

public class WorkOrderServiceTests
{
    private readonly Mock<IWorkOrderRepository> _workOrderRepo = new();
    private readonly Mock<IRepository<Vehicle>> _vehicleRepo = new();
    private readonly Mock<ISparePartRepository> _sparePartRepo = new();
    private readonly WorkOrderService _service;

    public WorkOrderServiceTests()
    {
        _service = new WorkOrderService(
            _workOrderRepo.Object,
            _vehicleRepo.Object,
            _sparePartRepo.Object,
            new DirectTransactionManager());
    }

    [Fact]
    public async Task CreateWorkOrder_ShouldThrow_WhenVehicleDoesNotExist()
    {
        var dto = CreateDto(vehicleId: 999);
        _vehicleRepo.Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Vehicle?)null);

        Func<Task> act = () => _service.CreateWorkOrderAsync(dto);

        var exception = await act.Should().ThrowAsync<KeyNotFoundException>();
        exception.Which.Message.Should().Be("Vehicle with ID 999 was not found.");
    }

    [Fact]
    public async Task CreateWorkOrder_ShouldDeductStockAndCalculateTotal()
    {
        var vehicle = CreateVehicle();
        var part = new SparePart { Id = 7, Name = "Oil filter", UnitPrice = 12.5m, StockQuantity = 8 };
        WorkOrder? addedOrder = null;

        _vehicleRepo.Setup(repo => repo.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);
        _sparePartRepo.Setup(repo => repo.GetForWorkOrderAsync(part.Id)).ReturnsAsync(part);
        _sparePartRepo.Setup(repo => repo.TryDecrementStockAsync(part.Id, 5)).ReturnsAsync(true);
        _workOrderRepo.Setup(repo => repo.AddAsync(It.IsAny<WorkOrder>()))
            .Callback<WorkOrder>(order => addedOrder = order)
            .Returns(Task.CompletedTask);
        _workOrderRepo.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.CreateWorkOrderAsync(new CreateWorkOrderDto(
            vehicle.Id,
            "Oil service",
            50m,
            [
                new WorkOrderPartRequestDto(part.Id, 2),
                new WorkOrderPartRequestDto(part.Id, 3)
            ]));

        result.TotalCost.Should().Be(112.5m);
        result.Status.Should().Be(nameof(WorkOrderStatus.InProgress));
        result.VehicleInfo.Should().Be("2020 Toyota Corolla (ABC123)");
        addedOrder.Should().NotBeNull();
        addedOrder!.UsedParts.Should().ContainSingle().Which.Quantity.Should().Be(5);
        _sparePartRepo.Verify(repo => repo.TryDecrementStockAsync(part.Id, 5), Times.Once);
        _workOrderRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateWorkOrder_ShouldReject_WhenStockIsInsufficient()
    {
        var vehicle = CreateVehicle();
        var part = new SparePart { Id = 7, Name = "Oil filter", UnitPrice = 12.5m, StockQuantity = 1 };

        _vehicleRepo.Setup(repo => repo.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);
        _sparePartRepo.Setup(repo => repo.GetForWorkOrderAsync(part.Id)).ReturnsAsync(part);

        Func<Task> act = () => _service.CreateWorkOrderAsync(new CreateWorkOrderDto(
            vehicle.Id,
            "Oil service",
            50m,
            [new WorkOrderPartRequestDto(part.Id, 2)]));

        await act.Should().ThrowAsync<ConflictException>();
        _workOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<WorkOrder>()), Times.Never);
        _workOrderRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateWorkOrder_ShouldReject_WhenStockChangesDuringReservation()
    {
        var vehicle = CreateVehicle();
        var part = new SparePart { Id = 7, Name = "Oil filter", UnitPrice = 12.5m, StockQuantity = 8 };

        _vehicleRepo.Setup(repo => repo.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);
        _sparePartRepo.Setup(repo => repo.GetForWorkOrderAsync(part.Id)).ReturnsAsync(part);
        _sparePartRepo.Setup(repo => repo.TryDecrementStockAsync(part.Id, 2)).ReturnsAsync(false);

        Func<Task> act = () => _service.CreateWorkOrderAsync(new CreateWorkOrderDto(
            vehicle.Id,
            "Oil service",
            50m,
            [new WorkOrderPartRequestDto(part.Id, 2)]));

        await act.Should().ThrowAsync<ConflictException>();
        _workOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<WorkOrder>()), Times.Never);
    }

    [Fact]
    public async Task CreateWorkOrder_ShouldReject_NegativeLaborCost()
    {
        var dto = new CreateWorkOrderDto(1, "Repair", -1m, []);

        Func<Task> act = () => _service.CreateWorkOrderAsync(dto);

        await act.Should().ThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>();
        _vehicleRepo.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetWorkOrderById_ShouldIncludeVehicleDetails()
    {
        var order = new WorkOrder
        {
            Id = 11,
            VehicleId = 5,
            Vehicle = CreateVehicle(),
            IssueDescription = "Repair",
            LaborCost = 20m,
            TotalCost = 20m,
            Status = WorkOrderStatus.InProgress
        };
        _workOrderRepo.Setup(repo => repo.GetByIdWithVehicleAsync(order.Id)).ReturnsAsync(order);

        var result = await _service.GetWorkOrderByIdAsync(order.Id);

        result.VehicleInfo.Should().Be("2020 Toyota Corolla (ABC123)");
    }

    [Fact]
    public async Task GetAllWorkOrders_ShouldIncludeVehicleDetails()
    {
        var order = new WorkOrder
        {
            Id = 11,
            VehicleId = 5,
            Vehicle = CreateVehicle(),
            IssueDescription = "Repair",
            Status = WorkOrderStatus.InProgress
        };
        _workOrderRepo.Setup(repo => repo.GetAllWithVehicleAsync())
            .ReturnsAsync(new[] { order });

        var result = (await _service.GetAllWorkOrdersAsync()).Should().ContainSingle().Which;

        result.VehicleInfo.Should().Be("2020 Toyota Corolla (ABC123)");
    }

    [Fact]
    public async Task CompleteWorkOrder_ShouldBeIdempotent_AndKeepOriginalCompletionTime()
    {
        var completedAt = DateTime.UtcNow.AddMinutes(-5);
        var order = new WorkOrder
        {
            Id = 11,
            VehicleId = 5,
            Vehicle = CreateVehicle(),
            Status = WorkOrderStatus.Completed,
            CompletedAt = completedAt
        };
        _workOrderRepo.Setup(repo => repo.GetByIdWithVehicleAsync(order.Id)).ReturnsAsync(order);

        var result = await _service.CompleteWorkOrderAsync(order.Id);

        result.Status.Should().Be(nameof(WorkOrderStatus.Completed));
        order.CompletedAt.Should().Be(completedAt);
        _workOrderRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CompleteWorkOrder_ShouldReject_CancelledOrder()
    {
        var order = new WorkOrder { Id = 11, Status = WorkOrderStatus.Cancelled };
        _workOrderRepo.Setup(repo => repo.GetByIdWithVehicleAsync(order.Id)).ReturnsAsync(order);

        Func<Task> act = () => _service.CompleteWorkOrderAsync(order.Id);

        await act.Should().ThrowAsync<ConflictException>();
        _workOrderRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    private static CreateWorkOrderDto CreateDto(int vehicleId) =>
        new(vehicleId, "Engine check", 100m, []);

    private static Vehicle CreateVehicle() => new()
    {
        Id = 5,
        Year = 2020,
        Make = "Toyota",
        Model = "Corolla",
        LicensePlate = "ABC123",
        OwnerName = "Ani",
        OwnerPhone = "555-0000"
    };

    private sealed class DirectTransactionManager : ITransactionManager
    {
        public Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation) =>
            operation();
    }
}