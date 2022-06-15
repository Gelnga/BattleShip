using System;
using Parser = TcParser.TcParser;

namespace BattleShipConsoleUI;

public static class CommonUiParts
{
    public static bool AreYouSureMenu(string message, string? differentMessage = null)
    {
        Console.Clear();
        if (differentMessage != null)
        {
            Parser.ParseColorAndPrint(differentMessage);
        }
        else
        {
            Parser.ParseColorAndPrint("Are you sure you want to " + Parser.White + message + Parser.White + " ?" + Parser.LineFeed);
        }
        Console.WriteLine("Press Y - yes, N - no");
        while (true)
        {
            ConsoleKeyInfo keyInput = Console.ReadKey();

            switch (keyInput.Key)
            {
                case ConsoleKey.Y:
                    return true;
                case ConsoleKey.N:
                    return false;
                default:
                    continue;
            }
        }
    }

    public static void AskUserToContinue()
    {
        Parser.ParseColorAndPrint(Parser.White +"     Press key " + 
                                  Parser.White +
                                  Parser.Magenta +
                                  "C" + 
                                  Parser.Magenta + 
                                  Parser.White + 
                                  " to continue" +
                                  Parser.White);
        
        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.C)
            {
                break;
            }
        }
    }
}