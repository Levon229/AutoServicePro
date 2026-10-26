using AutoServicePro.Application.DTOs;
using AutoServicePro.Application.Interfaces;
using AutoServicePro.Application.Services;
using AutoServicePro.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoServicePro.UnitTests;

public class WorkOrderServiceTests
{
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepoMock;
    private readonly Mock<IRepository<Vehicle>> _vehicleRepoMock;
    private readonly Mock<IRepository<SparePart>> _sparePartRepoMock;
    private readonly WorkOrderService _service;

    public WorkOrderServiceTests()
    {
        _workOrderRepoMock = new Mock<IRepository<WorkOrder>>();
        _vehicleRepoMock = new Mock<IRepository<Vehicle>>();
        _sparePartRepoMock = new Mock<IRepository<SparePart>>();

        _service = new WorkOrderService(
            _workOrderRepoMock.Object,
            _vehicleRepoMock.Object,
            _sparePartRepoMock.Object);
    }

    [Fact]
    public async Task CreateWorkOrder_ShouldThrowKeyNotFoundException_WhenVehicleDoesNotExist()
    {
        // Arrange: Prepare a DTO with a non-existent Vehicle ID
        var dto = new CreateWorkOrderDto(
            VehicleId: 999,
            IssueDescription: "Engine check",
            LaborCost: 15000m,
            PartsToUse: new List<WorkOrderPartRequestDto>()
        );

        // Vehicle repository returns null (vehicle not found)
        _vehicleRepoMock.Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Vehicle?)null);

        // Act & Assert: Verify that calling the method throws KeyNotFoundException
        Func<Task> act = async () => await _service.CreateWorkOrderAsync(dto);

        var exceptionAssertion = await act.Should().ThrowAsync<KeyNotFoundException>();
        exceptionAssertion.Which.Message.Should().Be("Vehicle with ID 999 was not found.");
    }
}