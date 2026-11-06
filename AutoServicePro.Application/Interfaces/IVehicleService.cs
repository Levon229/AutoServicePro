using AutoServicePro.Application.DTOs;

namespace AutoServicePro.Application.Interfaces;

public interface IVehicleService
{
    Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleDto dto);
    Task<VehicleResponseDto> GetVehicleByIdAsync(int id);
    Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync();
}