using System.Collections.Generic;
using System.Text.Json;

namespace BattleShipBrain
{
    public class GameConfig
    {
        public int BoardWidth { get; set; } = 10;
        public int BoardLength { get; set; } = 10;

        public List<ShipConfig> ShipConfigs { get; set; } = new()
        {
            new ShipConfig
            {
                Name = "Carrier",
                Quantity = 1,
                ShipSizeY = 1,
                ShipSizeX = 5
            },
            new ShipConfig
            {
                Name = "Battleship",
                Quantity = 2,
                ShipSizeY = 1,
                ShipSizeX = 4
            },
            new ShipConfig
            {
                Name = "Submarine",
                Quantity = 3,
                ShipSizeY = 1,
                ShipSizeX = 3
            },
            new ShipConfig
            {
                Name = "Cruiser",
                Quantity = 4,
                ShipSizeY = 1,
                ShipSizeX = 2
            },
            new ShipConfig
            {
                Name = "Patrol",
                Quantity = 5,
                ShipSizeY = 1,
                ShipSizeX = 1
            },
            
        };

        public EShipTouchRule EShipTouchRule { get; set; } = EShipTouchRule.NoTouch;

        public override string ToString()
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, jsonOptions);
        }
    }
}