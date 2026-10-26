namespace AutoServicePro.Application.DTOs;

public record CreatedVehicleDto(
    string Make,
    string Model,
    int Year,
    string LicensePlate,
    string VIN,
    string OwnerName,
    string OwnerPhone);

public record VehicleResponseDto(
    int Id,
    string Make,
    string Model,
    int Year,
    string LicensePlate,
    string VIN,
    string OwnerName,
    string OwnerPhone );