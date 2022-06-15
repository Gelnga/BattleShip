namespace TcParser;

// Text Color Parser
public static class TcParser
{
    private const string InRed = "r";
    private const string InYellow = "y";
    private const string InMagenta = "m";
    private const string InWhite = "w";
    private const string InDarkGray = "b";
    private const string InCol = "c";

    public const string Red = Identifier + InRed;
    public const string Yellow = Identifier + InYellow;
    public const string Magenta = Identifier + InMagenta;
    public const string White = Identifier + InWhite;
    public const string DarkGray = Identifier + InDarkGray;
    public const string Col = Identifier + InCol + Identifier + InCol;
    
    public const string LineFeed = "^";
    public const string Identifier = "%";
    
    public static void ParseColorAndPrint(string textToParse)
    {
        var marked = false;
        var colorChanged = false;
        var currentColor = "";
        foreach (var character in textToParse.ToCharArray().Select(x => x.ToString()))
        {
            switch (character)
            {
                case LineFeed:
                    Console.WriteLine();
                    continue;
                case Identifier:
                    marked = true;
                    continue;
            }

            if (marked)
            {
                switch (colorChanged)
                {
                    case true when currentColor == "%" + character:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        marked = false;
                        colorChanged = false;
                        currentColor = "";
                        continue;
                    case true when currentColor != "%" + character:
                        marked = false;
                        continue;
                }
                
                Console.ForegroundColor = character switch
                {
                    InWhite => ConsoleColor.White,
                    InYellow => ConsoleColor.Yellow,
                    InMagenta => ConsoleColor.Magenta,
                    InRed => ConsoleColor.Red,
                    InDarkGray => ConsoleColor.DarkGray,
                    _ => Console.ForegroundColor
                };

                if (character == InCol)
                {
                    Console.Write(":");
                }

                currentColor = Identifier + character;
                marked = false;
                colorChanged = true;
                continue;
            }
            
            Console.Write(character);
        }
    }
    public static string ClearText(string textToParse)
    {
        var clearedText = "";
        var colored = false;
        foreach (var character in textToParse.ToCharArray().Select(x => x.ToString()))
        {
            if (character == Identifier)
            {
                colored = true;
                continue;
            }

            if (colored)
            {
                colored = false;
            }
            else
            {
                clearedText += character;
            }
        }

        return clearedText;
    }

    public static string Decode(string stringToDecode)
    {
        var decoded = stringToDecode.Replace(Col, ":");
        return decoded;
    }

    public static string Encode(string stringToEncode)
    {
        var encoded = stringToEncode.Replace(":", Col);
        return encoded;
    }
}