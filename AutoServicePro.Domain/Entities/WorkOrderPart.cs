namespace AutoServicePro.Domain.Entities;

public class WorkOrderPart : BaseEntity
{
    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public int SparePartId { get; set; }
    public SparePart? SparePart { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceAtTime { get; set; }

}
