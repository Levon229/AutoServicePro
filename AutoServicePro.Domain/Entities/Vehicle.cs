namespace AutoServicePro.Domain.Entities;

public class Vehicle : BaseEntity
{
    

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public string OwerName { get; set; } = string.Empty;
    public string OwerPhone { get; set; } = string.Empty;
}
