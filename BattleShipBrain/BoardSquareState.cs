using Parser = TcParser.TcParser;

namespace BattleShipBrain

{
    public struct BoardSquareState
    {
        public bool IsShip { get; set; }
        public bool IsBomb { get; set; }

        public override string ToString()
        {
            return (IsShip, IsBomb) switch
            {
                (false, false) => " ~~~ ",
                (false, true) => Parser.Red + " ~X~ " + Parser.Red,
                (true, false) => Parser.White + " [ ] " + Parser.White,
                (true, true) => Parser.White + " [" + Parser.White + Parser.Red + "X" + Parser.Red +
                                Parser.White + "] " + Parser.White
            };
        }
        public string ToStringWeb()
        {
            return (IsShip, IsBomb) switch
            {
                (false, false) => " ",
                (false, true) => "X",
                (true, false) => "[  ]",
                (true, true) => "[X]"
            };
        }
    }
}