namespace AutoServicePro.Domain.Entities;

public  class SparePart : BaseEntity
{
    
    public string Name { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}