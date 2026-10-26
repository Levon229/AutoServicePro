using System.Runtime.CompilerServices;

namespace AutoServicePro.Application.DTOs;

public record CreateSparePartDto(
    string Name,
    string PartNumber,
    decimal UnitPrice,
    int StockQuantity);

public record SparePartResponseDto(
    int Id,
    string Name,
    string PartNumber,
    decimal UnitPrice,
    int StockQuantity);
