using System.ComponentModel.DataAnnotations;

namespace AutoServicePro.Application.DTOs;

public record CreateSparePartDto(
    [property: Required, StringLength(120)] string Name,
    [property: Required, StringLength(80)] string PartNumber,
    [property: Range(typeof(decimal), "0", "999999999")] decimal UnitPrice,
    [property: Range(0, int.MaxValue)] int StockQuantity);

public record SparePartResponseDto(
    int Id,
    string Name,
    string PartNumber,
    decimal UnitPrice,
    int StockQuantity);