using System.ComponentModel.DataAnnotations;
using AutoServicePro.Application.DTOs;
using AutoServicePro.Application.Exceptions;
using AutoServicePro.Application.Interfaces;
using AutoServicePro.Domain.Entities;

namespace AutoServicePro.Application.Services;

public sealed class SparePartService(IRepository<SparePart> sparePartRepo) : ISparePartService
{
    public async Task<SparePartResponseDto> CreateSparePartAsync(CreateSparePartDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.PartNumber))
            throw new ValidationException("Spare part name and part number are required.");

        if (dto.Name.Length > 120 || dto.PartNumber.Length > 80)
            throw new ValidationException("Spare part name or part number exceeds the allowed length.");

        if (dto.UnitPrice < 0 || dto.UnitPrice > 999999999m || dto.StockQuantity < 0)
            throw new ValidationException("Price or stock quantity is outside the supported range.");

        var normalizedPartNumber = dto.PartNumber.Trim().ToUpperInvariant();
        var existingParts = await sparePartRepo.GetAllAsync();
        if (existingParts.Any(part =>
                string.Equals(part.PartNumber.Trim(), normalizedPartNumber, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException("A spare part with this part number already exists.");
        }

        var sparePart = new SparePart
        {
            Name = dto.Name.Trim(),
            PartNumber = normalizedPartNumber,
            UnitPrice = dto.UnitPrice,
            StockQuantity = dto.StockQuantity
        };

        await sparePartRepo.AddAsync(sparePart);
        await sparePartRepo.SaveChangesAsync();

        return ToResponse(sparePart);
    }

    public async Task<SparePartResponseDto> GetSparePartByIdAsync(int id)
    {
        var part = await sparePartRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Spare part with ID {id} was not found.");

        return ToResponse(part);
    }

    public async Task<IEnumerable<SparePartResponseDto>> GetAllSparePartsAsync()
    {
        var parts = await sparePartRepo.GetAllAsync();
        return parts.Select(ToResponse);
    }

    private static SparePartResponseDto ToResponse(SparePart part) => new(
        part.Id,
        part.Name,
        part.PartNumber,
        part.UnitPrice,
        part.StockQuantity);
}