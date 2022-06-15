using System.Collections.Generic;

namespace BattleShipBrain
{
    // DTO - Data Transfer Object
    public class SaveGameDto
    {
        public int CurrentPlayerId { get; set; }
        public bool AgainstAi { get; set; }
        public GameBoardDto[] GameBoards { get; set; } = {new(), new()};
        public class GameBoardDto
        {
            public List<List<BoardSquareState>> Board { get; set; } = null!;
            public List<Ship> Ships { get; set; } = null!;
        }
    }
    
}