using System;
using System.Collections.Generic;
using System.Linq;
using BattleShipBrain;
using Parser = TcParser.TcParser;

namespace BattleShipConsoleUI;
public class BsConsoleUi
{
    private readonly BsBrain _gameBrain;
    private static readonly List<char> Alphabet = BattleShipConsoleUI.Alphabet.GetAlphabet();
    public BsConsoleUi(BsBrain gameBrain)
    {
        _gameBrain = gameBrain;
    }

    private static void DrawBoard(BoardSquareState[,] board, bool hideAfloatShips = false,
        Dictionary<string, string>? highlightedCords = null) 
    {
        Console.Write("|MMM|");
        for (var x = 0; x < board.GetLength(0); x++)
        {
            Console.Write("--" + Alphabet[x] + "--");
        }
        Console.Write("|");
        Console.WriteLine();
            
        for (var y = 0; y < board.GetLength(1); y++)
        {

            if (y + 1 > 9)
            {
                Console.Write("| " + (y + 1) + "|");
            }
            else
            {
                Console.Write("| " + (y + 1) + " |");
            }

            for (var x = 0; x < board.GetLength(0); x++)
            {
                // Hide ships on enemy board
                if (hideAfloatShips && board[x, y].IsBomb == false)
                {
                    board[x, y].IsShip = false;
                }
                
                // Highlight chosen cords
                if (highlightedCords != null && highlightedCords.ContainsKey(new Coordinate(x, y).ToString()))
                {
                    var cord = new Coordinate(x, y).ToString();
                    Parser.ParseColorAndPrint(highlightedCords[cord] + Parser.ClearText(board[x, y].ToString()) +
                                              highlightedCords[cord]);
                        
                } else {
                    Parser.ParseColorAndPrint(board[x, y].ToString());
                }
            }
            Console.Write("|");
            Console.WriteLine();
        }
            
        Console.Write("|WWW|");
        for (var x = 0; x < board.GetLength(0); x++)
        {
            Console.Write("-----");
        }
        Console.Write("|");
        Parser.ParseColorAndPrint(Parser.LineFeed + Parser.LineFeed);
    }
    // This method overload is made to match Menu class object's RunMethod value type Action
    public void DrawInGamePlayerView()
    {
        // -1 -1 or any other out of game board borders coordinates input disables BoardSquare highlighting
        DrawInGamePlayerView(new Dictionary<string, string>());
    }
    // ReSharper disable once MethodOverloadWithOptionalParameter
    private void DrawInGamePlayerView(Dictionary<string, string> highlightedSquares)
    {
        var opponentId = _gameBrain.GetOpponentPlayerId();
        var playerId = _gameBrain.GetCurrentPlayerId();

        Parser.ParseColorAndPrint("    Player " + playerId + "'s move" + Parser.LineFeed + Parser.LineFeed);
        Parser.ParseColorAndPrint("    Opponent's board    " + Parser.LineFeed + Parser.LineFeed);

        // Get opponent sunken ships
        HighLightChosenCords(highlightedSquares, SunkenShipsByPlayerId(_gameBrain.GetOpponentPlayerId()),
            Parser.DarkGray);
        DrawBoard(_gameBrain.GetBoard(opponentId), true, highlightedSquares);

        // Get current player sunken ships
        highlightedSquares = new Dictionary<string, string>();
        HighLightChosenCords(highlightedSquares, SunkenShipsByPlayerId(_gameBrain.GetCurrentPlayerId()),
            Parser.DarkGray);

        Parser.ParseColorAndPrint("   Your board    " + Parser.LineFeed + Parser.LineFeed);
        
        DrawBoard(_gameBrain.GetBoard(playerId), false, highlightedSquares);
    }

    private List<String> SunkenShipsByPlayerId(int id)
    {
        return new List<string>(_gameBrain.GetListOfSunkenShipsCoordinatesByPlayerId(id).
            Select(x => x.ToString()));
    }

    public void PlaceShipsOnCurrentPlayerBoard()
    {
        _gameBrain.InitShipPlacement();
        while (!_gameBrain.AreShipsPlacedOnCurrentBoard()) {
            
            var shipPreviewCords = new List<string>(_gameBrain
                .GetShipPlacementCordsPreview()
                .Select(x => x.ToString()));
                    
            var highlightedShipPreviewCords = new Dictionary<string, string>();
            HighLightChosenCords(highlightedShipPreviewCords, shipPreviewCords, Parser.Yellow);

            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("   Your board, Player " + _gameBrain.GetCurrentPlayerId() + "    ");
            Console.WriteLine();
            DrawShipPlacementHints();
            DrawBoard(_gameBrain.GetBoard(_gameBrain.GetCurrentPlayerId()), false,
                highlightedShipPreviewCords);

            var keyInput = Console.ReadKey().Key;

            switch (keyInput)
            {
                case ConsoleKey.G:
                    if (_gameBrain.CurrentPlayerHasShipsOnBoard())
                    {
                        var answer =
                            CommonUiParts.AreYouSureMenu("stop placing ships and let game generate them for you");
                        if (!answer)
                        {
                            break;
                        }
                    }

                    _gameBrain.GenerateShipPlacementOnCurrentBoard();
                    _gameBrain.SwapCurrentPlayerId();
                    return;

                case ConsoleKey.T:
                    _gameBrain.RotateShip(1);

                    break;
                case ConsoleKey.R:
                    _gameBrain.RotateShip(-1);

                    break;
                case ConsoleKey.U:
                    _gameBrain.UndoLastShipPlacementOnCurrentBoard();
                            
                    break;
                case ConsoleKey.I:
                    _gameBrain.RedoLastShipPlacementOnCurrentBoard();
                            
                    break;
                case ConsoleKey.Enter:
                    _gameBrain.PlaceShip();

                    break;
                default:
                    var validatedCords = HighlightedSquareControl(keyInput, _gameBrain.GetShipPlacementPreviewCoordinate(),
                        false);
                    if (validatedCords != null && _gameBrain.ValidateShipPlacementInsideGameBoardBoarders(validatedCords.Value))
                    {
                        _gameBrain.UpdateShipPlacementPreviewCoordinate(validatedCords.Value);
                    }

                    break;
            }
        }
            
        _gameBrain.SwapCurrentPlayerId();
        _gameBrain.ClearPreviouslyPlacedShips();
    }

    private void HighLightChosenCords(Dictionary<string, string> highlightedCords, List<string> cords,
        string highlight)
    {
        foreach (var cord in cords.Where(cord => !highlightedCords.ContainsKey(cord)))
        {
            highlightedCords.Add(cord, highlight);
        }
    }
    public string MakeMove()
    {
        var aiMoves = _gameBrain.DoesAiMoveNow(); 
        var moveMade = false;
        Coordinate highlightedCords;
        highlightedCords =  _gameBrain.GetCurrentPlayerLastMove();

        var isEnded = false;

        do
        {

            Console.Clear();
            Console.WriteLine();
            if (!aiMoves)
            {
                DrawInGamePlayerView(new Dictionary<string, string>
                {
                    { highlightedCords.ToString(), Parser.Yellow }
                });
            }
            Console.WriteLine();

            if (aiMoves)
            {
                highlightedCords = _gameBrain.AiMakeMove();
            }

            var keyInput = aiMoves ? ConsoleKey.Enter : Console.ReadKey().Key;

            switch (keyInput)
            {
                case ConsoleKey.Enter:
                    moveMade = _gameBrain.Fire(highlightedCords.X, highlightedCords.Y);
                        
                    break;
                default:
                    var validatedCords = HighlightedSquareControl(keyInput, highlightedCords);
                    highlightedCords = validatedCords ?? highlightedCords;
                        
                    break;
            }

            if (!_gameBrain.HasCurrentPlayerWon()) continue;
            isEnded = true;
            break;
            
        } while (!moveMade);
            
        Console.Clear();
        if (!isEnded)
        {
            _gameBrain.SwapCurrentPlayerId();
            if (_gameBrain.DoesAiMoveNow())
            {
                MakeMove();
            }
            else if (!_gameBrain.IsGameAgainstAi())
            {
                Console.WriteLine("Pass move to the next player. Next player - press any key to continue");
                Console.ReadKey();
            
                return "C";
            }
        }
        else
        {
            Console.Clear();
            Console.WriteLine();
            if (_gameBrain.IsGameAgainstAi())
            {
                if (_gameBrain.GetCurrentPlayerId() == 2)
                {
                    Console.Write("     AI won!");
                }
                else
                {
                    Console.WriteLine("     You won!");
                }
            }
            else
            {
                Console.WriteLine("     Player " + _gameBrain.GetCurrentPlayerId() + " won!");
            }

            Console.WriteLine();
            Console.WriteLine(_gameBrain.IsGameAgainstAi() ? "     Your board" : "     Player 1's board");
            Console.WriteLine();
            
            var highlightedSquares = new Dictionary<string, string>();
            HighLightChosenCords(highlightedSquares, SunkenShipsByPlayerId(1), Parser.DarkGray);
            DrawBoard(_gameBrain.GetBoard(1), false, highlightedSquares);
            
            Console.WriteLine();
            Console.WriteLine(_gameBrain.IsGameAgainstAi() ? "     AI board" : "     Player 2's board");
            Console.WriteLine();
            
            highlightedSquares = new Dictionary<string, string>();
            HighLightChosenCords(highlightedSquares, SunkenShipsByPlayerId(2), Parser.DarkGray);
            DrawBoard(_gameBrain.GetBoard(2), false, highlightedSquares);
            
            Console.WriteLine();
            CommonUiParts.AskUserToContinue();
            return "MM";
        }

        return "C";
    }

    private Coordinate? HighlightedSquareControl(ConsoleKey keyInput, Coordinate highlightedCords, 
        bool borderOverflow = true)
    {
        Coordinate? highlightedCordsValidated = null;
        var validated = true;
            
        // For some reason if inside lambda expression a constructor value is depicted as variable++ or variable-- 
        // program will first create an instance of an object and only then increment the value
        switch (keyInput)
        {
            case ConsoleKey.LeftArrow:
                highlightedCordsValidated = ValidateHighlightedSquareCords(highlightedCords,
                    coordinate => new Coordinate(coordinate.X - 1, coordinate.Y),
                    borderOverflow);
                    
                break;
            case ConsoleKey.RightArrow:
                highlightedCordsValidated = ValidateHighlightedSquareCords(highlightedCords,
                    coordinate => new Coordinate(coordinate.X + 1, coordinate.Y),
                    borderOverflow);

                break;
            case ConsoleKey.DownArrow:
                highlightedCordsValidated = ValidateHighlightedSquareCords(highlightedCords,
                    coordinate => new Coordinate(coordinate.X, coordinate.Y + 1),
                    borderOverflow);

                break;
            case ConsoleKey.UpArrow:
                highlightedCordsValidated = ValidateHighlightedSquareCords(highlightedCords,
                    coordinate => new Coordinate(coordinate.X, coordinate.Y - 1),
                    borderOverflow);
                    
                break;
            default:
                validated = false;
                break;
        }

        return validated ? highlightedCordsValidated : null;
    }
    private Coordinate? ValidateHighlightedSquareCords(Coordinate cords, 
        Func<Coordinate, Coordinate> cordsModification, 
        bool borderOverflow)
    {
        cords = cordsModification(cords);
        var xMaxValue = _gameBrain.GetBoard(1).GetLength(0) - 1;
        var yMaxValue = _gameBrain.GetBoard(1).GetLength(1) - 1;
            
        // This code part is used to move highlight to the opposite side of a
        // game board border if highlight exceeds it
        if (borderOverflow)
        {
            if (cords.X < 0)
            {
                cords.X = xMaxValue;
            }

            if (cords.X > xMaxValue)
            {
                cords.X = 0;
            }

            if (cords.Y < 0)
            {
                cords.Y = yMaxValue;
            }

            if (cords.Y > yMaxValue)
            {
                cords.Y = 0;
            }

            return cords;
        }
        if (cords.X >= 0 && cords.X <= _gameBrain.GetBoard(1).GetLength(0) - 1 &&
            cords.Y >= 0 && cords.Y <= _gameBrain.GetBoard(1).GetLength(1) - 1)
        {
            return cords;
        }

        return null;
    }
    
    private static void DrawShipPlacementHints()
    {
        Parser.ParseColorAndPrint("     " + 
                                  Parser.White +
                                  "Use " +
                                  Parser.White +
                                  Parser.Magenta +
                                  "arrow" +
                                  Parser.Magenta +
                                  Parser.White +
                                  " keys to move a ship. To place a ship press " +
                                  Parser.White +
                                  Parser.Magenta +
                                  "Enter" +
                                  Parser.Magenta +
                                  Parser.LineFeed + Parser.LineFeed);
        
        Parser.ParseColorAndPrint("     " + 
                                  Parser.White +
                                  "Use " + 
                                  Parser.White + 
                                  Parser.Magenta + 
                                  "R" + 
                                  Parser.Magenta +
                                  Parser.White + 
                                  " and " + 
                                  Parser.White +
                                  Parser.Magenta +
                                  "T" +
                                  Parser.Magenta +
                                  Parser.White + 
                                  " keys to rotate a ship. If ship is to close to a " +
                                  Parser.LineFeed + 
                                  "     game board border and in case of rotation it would exceed boarder, " +
                                  "then ship will not rotate. To rotate it, move it deeper into the game board" +
                                  Parser.White + Parser.LineFeed + Parser.LineFeed);
        
        Parser.ParseColorAndPrint("     " + Parser.White + "You can undo last ship placement by pressing " +
                                  Parser.White +
                                  Parser.Magenta +
                                  "U" + 
                                  Parser.Magenta +
                                  Parser.White +
                                  " key and redo ship placement by pressing " + 
                                  Parser.White +
                                  Parser.Magenta + 
                                  "I" + 
                                  Parser.Magenta +
                                  Parser.White + 
                                  " key"
                                  + Parser.White + Parser.LineFeed + Parser.LineFeed);
        
        Parser.ParseColorAndPrint("     " + Parser.White + 
                                  "Press " +
                                  Parser.White +
                                  Parser.Magenta +
                                  "G" + 
                                  Parser.Magenta +
                                  Parser.White +
                                  " key if you want to automatically generate ship placement" +
                                  Parser.White + Parser.LineFeed + Parser.LineFeed);
    }

    public void LaunchTutorial()
    {
        Console.Clear();
        var answer = CommonUiParts.AreYouSureMenu("", Parser.White + "Do you want to read game rules?" + 
                                         Parser.White + Parser.LineFeed);
        Console.Clear();
        if (answer)
        {
            LaunchGameRulesTutorial();
            Console.Clear();
        }
        
        answer = CommonUiParts.AreYouSureMenu("", Parser.White + "Do you want to watch controls tutorial" + 
                                                      Parser.White + Parser.LineFeed);
        Console.Clear();
        if (!answer) return;
        LaunchGameControlsTutorial();
        Console.Clear();
    }

    private void LaunchGameRulesTutorial()
    {
        var exampleBlankSquareState = new BoardSquareState();
        
        var exampleMissSquareState = new BoardSquareState
        {
            IsBomb = true
        };

        var exampleShipSquareState = new BoardSquareState
        {
            IsShip = true
        };
        
        var exampleHitShipSquareState = new BoardSquareState
        {
            IsBomb = true,
            IsShip = true
        };
        var exampleSunkenShip = Parser.DarkGray + "[X]" + Parser.DarkGray;

        Parser.ParseColorAndPrint(Parser.LineFeed + Parser.White +
                                  "     Before game starts, every player must place ships on their board." +
                                  Parser.LineFeed +
                                  "     By default placed ships shouldn't touch each other " +
                                  Parser.LineFeed +
                                  "     (squares, that are adjacent to ship corners and sides  must be empty." +
                                  " These rules can be configured in the settings)" +
                                  Parser.LineFeed + Parser.LineFeed +
                                  "     The game starts with the player 1 making a first move. During a move player " +
                                  "chooses a square on an enemy board, where he will place a bomb." +
                                  Parser.LineFeed +
                                  "     (current player can't see ships on enemy board, if they wasn't hit)." +
                                  Parser.LineFeed +
                                  "     If he hits an enemy ship " +
                                  "with a bomb or completely destroys it, he will be granted one more move." +
                                  Parser.LineFeed +
                                  "     If a player misses, his opponent makes move. " +
                                  Parser.LineFeed + Parser.LineFeed +
                                  "     Gameboards are split on squares, that can have different states." + 
                                  Parser.LineFeed +
                                  "     Here is a list of different board square states, that can be encountered during the game:" +
                                  Parser.LineFeed + Parser.LineFeed +
                                  
                                  "     Blank board square state: " + Parser.White + exampleBlankSquareState + Parser.White +
                                  Parser.LineFeed + 
                                  "     Missed hit square state: " + Parser.White + exampleMissSquareState + Parser.White +
                                  Parser.LineFeed + 
                                  "     Ship square state: " + Parser.White + exampleShipSquareState + Parser.White +
                                  Parser.LineFeed + 
                                  "     Hit ship square state: " + Parser.White + exampleHitShipSquareState + Parser.White +
                                  Parser.LineFeed + 
                                  "     Dead ship: " + Parser.White + exampleSunkenShip + exampleSunkenShip + exampleSunkenShip +
                                  Parser.White + Parser.LineFeed + Parser.LineFeed +
                                  
                                  "     This player, who will first destroy all enemy ships will win!" +
                                  Parser.LineFeed + Parser.LineFeed);
        
        CommonUiParts.AskUserToContinue();
    }

    private void LaunchGameControlsTutorial()
    {
        Parser.ParseColorAndPrint(Parser.LineFeed + Parser.White +
                                  "     Before every move, player can navigate in ingame menu for various options like " +
                                  "saving the game and exiting. " +
                                  Parser.LineFeed +
                                  "     To make a move player should choose corresponding " +
                                  "option in ingame menu" +
                                  Parser.LineFeed + Parser.LineFeed +
                                  "     When player chooses to make a move, he will see enemy board on top and his board on " +
                                  "bottom. Square, that is highlighted with" +
                                  Parser.White + Parser.Yellow +
                                  " yellow, " +
                                  Parser.Yellow + Parser.White + Parser.LineFeed +
                                  "     is that square where current player will place a bomb." + 
                                  Parser.LineFeed + Parser.LineFeed +
                                  "     To highlight different square, use " +
                                  Parser.White + 
                                  Parser.Magenta +
                                  "arrow" + 
                                  Parser.Magenta + 
                                  Parser.White +
                                  " keys. To make a move press " +
                                  Parser.White + 
                                  Parser.Magenta + 
                                  "enter" + 
                                  Parser.Magenta + 
                                  Parser.White + 
                                  ". If current misses, " +
                                  "move immediately is passed to an opponent player" +
                                  Parser.LineFeed + Parser.LineFeed);
        
        CommonUiParts.AskUserToContinue();
    }
}