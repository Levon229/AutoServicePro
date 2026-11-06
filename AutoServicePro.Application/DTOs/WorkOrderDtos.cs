using System.ComponentModel.DataAnnotations;

namespace AutoServicePro.Application.DTOs;

public record WorkOrderPartRequestDto(
    [property: Range(1, int.MaxValue)] int SparePartId,
    [property: Range(1, int.MaxValue)] int Quantity);

public record CreateWorkOrderDto(
    [property: Range(1, int.MaxValue)] int VehicleId,
    [property: Required, StringLength(1000)] string IssueDescription,
    [property: Range(typeof(decimal), "0", "999999999")] decimal LaborCost,
    [property: Required] List<WorkOrderPartRequestDto> PartsToUse);

public record WorkOrderResponseDto(
    int Id,
    int VehicleId,
    string VehicleInfo,
    string IssueDescription,
    decimal LaborCost,
    decimal TotalCost,
    string Status,
    DateTime CreatedAt);