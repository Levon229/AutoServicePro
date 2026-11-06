using System.ComponentModel.DataAnnotations;

namespace AutoServicePro.Application.DTOs;

public record CreateVehicleDto(
    [property: Required, StringLength(60)] string Make,
    [property: Required, StringLength(60)] string Model,
    [property: Range(1886, 2100)] int Year,
    [property: Required, StringLength(24)] string LicensePlate,
    [property: Required, StringLength(32)] string VIN,
    [property: Required, StringLength(120)] string OwnerName,
    [property: Required, StringLength(40)] string OwnerPhone);

public record VehicleResponseDto(
    int Id,
    string Make,
    string Model,
    int Year,
    string LicensePlate,
    string VIN,
    string OwnerName,
    string OwnerPhone);