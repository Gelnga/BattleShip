using System;
using System.Collections.Generic;

namespace BattleShipConsoleUI;

public static class Alphabet
{
    public static List<char> GetAlphabet()
    {
        var alphabet = new List<char>();
        
        for (var i = 65; i <= 90; i++) {
            alphabet.Add(Convert.ToChar(i));
        }

        return alphabet;
    }
}