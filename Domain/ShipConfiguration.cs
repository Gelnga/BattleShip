using System.ComponentModel.DataAnnotations;

namespace Domain;

public class ShipConfiguration
{
    public int Id { get; set; }

    [MaxLength(255)]
    public string ShipName { get; set; } = default!;
    public int ShipQuantity { get; set; }
    public int ShipSizeX { get; set; }
    public int ShipSizeY { get; set; }
}