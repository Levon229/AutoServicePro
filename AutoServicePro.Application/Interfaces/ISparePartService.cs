using AutoServicePro.Application.DTOs;

namespace AutoServicePro.Application.Interfaces;

public interface ISparePartService
{
    Task<SparePartResponseDto> CreateSparePartAsync(CreateSparePartDto dto);
    Task<SparePartResponseDto> GetSparePartByIdAsync(int id);
    Task<IEnumerable<SparePartResponseDto>> GetAllSparePartsAsync();
}