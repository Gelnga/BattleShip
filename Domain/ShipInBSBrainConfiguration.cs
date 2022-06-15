namespace Domain;

public class ShipInBSBrainConfiguration
{
    public int Id { get; set; }
    
    public int BsBrainConfigurationId { get; set; }
    public BsBrainConfiguration BsBrainConfiguration { get; set; } = default!;
    
    public int ShipConfigurationId { get; set; }
    public ShipConfiguration ShipConfiguration { get; set; } = default!;
}