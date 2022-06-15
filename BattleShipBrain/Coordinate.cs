using System.Text.Json;

namespace BattleShipBrain
{
    public struct Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"X: {X}, Y: {Y}";

        public string GetCoordinateJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public void RestoreCoordinateFromJson(string json)
        {
            var restoredCord = JsonSerializer.Deserialize<Coordinate>(json);
            X = restoredCord.X;
            Y = restoredCord.Y;
        }
    }
}