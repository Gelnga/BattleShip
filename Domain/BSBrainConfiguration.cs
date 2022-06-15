using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain;

public class BsBrainConfiguration
{
    public int Id { get; set; }
    
    public int BoardWidth { get; set; }
    public int BoardLength { get; set; }
    [MaxLength(255)] 
    public string EShipTouchRule { get; set; } = default!;
    
    public ICollection<SavedBsBrain>? SavedBsBrains { get; set; }
    public ICollection<ShipInBSBrainConfiguration>? ShipInBsBrainConfigurations { get; set; }
}