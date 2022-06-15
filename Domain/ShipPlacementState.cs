using System.ComponentModel.DataAnnotations;

namespace Domain;

public class ShipPlacementState
{
    public int Id { get; set; }
    
    public int SavedBsBrainId { get; set; }
    public SavedBsBrain SavedBsBrain { get; set; } = default!;
    
    public int Rotation { get; set; }
    public int ShipConfigIndex { get; set; }
    public int CurrentShipQuantity { get; set; }
    [MaxLength(32)] 
    public string ShipPlacementPreviewCoordinate { get; set; } = default!;
    [MaxLength(20000)]
    public string PreviouslyPlacedShips { get; set; } = default!;
}