using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain;

public class SavedBsBrain
{
    public int Id { get; set; }
    
    public int BsBrainConfigurationId { get; set; }
    public BsBrainConfiguration? BsBrainConfiguration { get; set; }
    
    public ICollection<ShipPlacementState>? ShipPlacementStates { get; set; }

    [MaxLength(100000)]
    public string GameBoards { get; set; } = default!;
    public bool AgainstAi { get; set; }
    public int CurrentPlayerId { get; set; }
    [MaxLength(1000)]
    public string SavedBrainName { get; set; } = default!;
}