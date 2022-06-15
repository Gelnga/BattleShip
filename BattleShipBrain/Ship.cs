using System.Collections.Generic;
using System.Linq;

namespace BattleShipBrain
{
    public class Ship
    {
        public string Name { get; set; }
        public List<Coordinate> Coordinates { get; set; }
        
        public Ship(string name, List<Coordinate> coordinates)
        {
            Name = name;
            Coordinates = coordinates;
        }
        
        public bool IsShipSunken(BoardSquareState[,] board) =>
            Coordinates.All(coordinate => board[coordinate.X, coordinate.Y].IsBomb);
    }
}