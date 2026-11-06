using AutoServicePro.Application.DTOs;
using AutoServicePro.Application.Exceptions;
using AutoServicePro.Application.Interfaces;
using AutoServicePro.Domain.Entities;

namespace AutoServicePro.Application.Services;

public sealed class VehicleService(IRepository<Vehicle> vehicleRepo) : IVehicleService
{
    public async Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Validate(dto);

        var normalizedPlate = Normalize(dto.LicensePlate);
        var normalizedVin = Normalize(dto.VIN);
        var existingVehicles = await vehicleRepo.GetAllAsync();

        if (existingVehicles.Any(vehicle =>
                Normalize(vehicle.LicensePlate) == normalizedPlate ||
                Normalize(vehicle.VIN) == normalizedVin))
        {
            throw new ConflictException("A vehicle with this license plate or VIN already exists.");
        }

        var vehicle = new Vehicle
        {
            Make = dto.Make.Trim(),
            Model = dto.Model.Trim(),
            Year = dto.Year,
            LicensePlate = normalizedPlate,
            VIN = normalizedVin,
            OwnerName = dto.OwnerName.Trim(),
            OwnerPhone = dto.OwnerPhone.Trim()
        };

        await vehicleRepo.AddAsync(vehicle);
        await vehicleRepo.SaveChangesAsync();

        return ToResponse(vehicle);
    }

    public async Task<VehicleResponseDto> GetVehicleByIdAsync(int id)
    {
        var vehicle = await vehicleRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Vehicle with ID {id} was not found.");

        return ToResponse(vehicle);
    }

    public async Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync()
    {
        var vehicles = await vehicleRepo.GetAllAsync();
        return vehicles.Select(ToResponse);
    }

    private static void Validate(CreateVehicleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Make) ||
            string.IsNullOrWhiteSpace(dto.Model) ||
            string.IsNullOrWhiteSpace(dto.LicensePlate) ||
            string.IsNullOrWhiteSpace(dto.VIN) ||
            string.IsNullOrWhiteSpace(dto.OwnerName) ||
            string.IsNullOrWhiteSpace(dto.OwnerPhone))
        {
            throw new ArgumentException("Vehicle fields cannot be blank.");
        }

        if (dto.Make.Length > 60 || dto.Model.Length > 60 ||
            dto.LicensePlate.Length > 24 || dto.VIN.Length > 32 ||
            dto.OwnerName.Length > 120 || dto.OwnerPhone.Length > 40)
        {
            throw new ArgumentException("One or more vehicle fields exceed the allowed length.");
        }

        if (dto.Year < 1886 || dto.Year > DateTime.UtcNow.Year + 1)
            throw new ArgumentException("Vehicle year is outside the supported range.");
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static VehicleResponseDto ToResponse(Vehicle vehicle) => new(
        vehicle.Id,
        vehicle.Make,
        vehicle.Model,
        vehicle.Year,
        vehicle.LicensePlate,
        vehicle.VIN,
        vehicle.OwnerName,
        vehicle.OwnerPhone);
}