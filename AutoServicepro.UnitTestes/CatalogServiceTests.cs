using AutoServicePro.Application.DTOs;
using AutoServicePro.Application.Exceptions;
using AutoServicePro.Application.Interfaces;
using AutoServicePro.Application.Services;
using AutoServicePro.Domain.Entities;
using FluentAssertions;
using Moq;

namespace AutoServicePro.UnitTests;

public class CatalogServiceTests
{
    [Fact]
    public async Task CreateVehicle_ShouldNormalizeIdentifiers()
    {
        var repository = new Mock<IRepository<Vehicle>>();
        repository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(Array.Empty<Vehicle>());
        repository.Setup(repo => repo.AddAsync(It.IsAny<Vehicle>())).Returns(Task.CompletedTask);
        repository.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);
        var service = new VehicleService(repository.Object);
        Vehicle? addedVehicle = null;
        repository.Setup(repo => repo.AddAsync(It.IsAny<Vehicle>()))
            .Callback<Vehicle>(vehicle => addedVehicle = vehicle)
            .Returns(Task.CompletedTask);

        var result = await service.CreateVehicleAsync(new CreateVehicleDto(
            " Toyota ",
            " Corolla ",
            2020,
            " abc123 ",
            " vin-001 ",
            " Ani ",
            " 555-0101 "));

        result.LicensePlate.Should().Be("ABC123");
        result.VIN.Should().Be("VIN-001");
        addedVehicle!.OwnerName.Should().Be("Ani");
    }

    [Fact]
    public async Task CreateVehicle_ShouldRejectDuplicateIdentifiers()
    {
        var repository = new Mock<IRepository<Vehicle>>();
        repository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new[]
        {
            new Vehicle { LicensePlate = "ABC123", VIN = "VIN-001" }
        });
        var service = new VehicleService(repository.Object);

        Func<Task> act = () => service.CreateVehicleAsync(new CreateVehicleDto(
            "Toyota", "Corolla", 2020, "abc123", "another-vin", "Ani", "555-0101"));

        await act.Should().ThrowAsync<ConflictException>();
        repository.Verify(repo => repo.AddAsync(It.IsAny<Vehicle>()), Times.Never);
    }

    [Fact]
    public async Task CreateSparePart_ShouldRejectNegativePrice()
    {
        var repository = new Mock<IRepository<SparePart>>();
        var service = new SparePartService(repository.Object);

        Func<Task> act = () => service.CreateSparePartAsync(
            new CreateSparePartDto("Oil", "OIL-1", -1m, 5));

        await act.Should().ThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>();
        repository.Verify(repo => repo.AddAsync(It.IsAny<SparePart>()), Times.Never);
    }

    [Fact]
    public async Task CreateSparePart_ShouldRejectDuplicatePartNumber()
    {
        var repository = new Mock<IRepository<SparePart>>();
        repository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new[]
        {
            new SparePart { Name = "Oil", PartNumber = "OIL-1" }
        });
        var service = new SparePartService(repository.Object);

        Func<Task> act = () => service.CreateSparePartAsync(
            new CreateSparePartDto("Oil", " oil-1 ", 12m, 5));

        await act.Should().ThrowAsync<ConflictException>();
        repository.Verify(repo => repo.AddAsync(It.IsAny<SparePart>()), Times.Never);
    }
}