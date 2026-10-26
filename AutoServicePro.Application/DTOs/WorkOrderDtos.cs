namespace AutoServicePro.Application.DTOs;


    public record WorkOrderPartRequestDto(
    int SparePartId,
    int Quantity);

    public record CreateWorkOrderDto(
        int VehicleId,
        string IssueDescription,
        decimal LaborCost,
        List<WorkOrderPartRequestDto> PartsToUse);

    public record WorkOrderResponseDto(
        int Id,
        int VehicleId,
        string VehicleInfo,
        string IssueDescription,
        decimal LaborCost,
        decimal TotalCost,
        string Status,
        DateTime CreatedAt);

