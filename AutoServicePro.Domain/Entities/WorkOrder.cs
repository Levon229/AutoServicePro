using AutoServicePro.Domain.Enums;

namespace AutoServicePro.Domain.Entities;

public class WorkOrder : BaseEntity
{
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public decimal LaborCost { get; set; }
    public decimal TotalCost { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Peding;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ICollection<WorkOrderPart> UsedParts { get; set; } = new List<WorkOrderPart>();


}
