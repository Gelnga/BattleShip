using System;
using Parser = TcParser.TcParser;

namespace BattleShipConsoleUI
{
    public class MenuItem 
    {
        public MenuItem(string shortCut, string title, Func<string> runMethod)
        {
            if (string.IsNullOrEmpty(shortCut))
            {
                throw new ArgumentException("shortCut cannot be empty!");
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("title cannot be empty!");
            }
            
            ShortCut = shortCut.Trim();
            Title = title.Trim();
            RunMethod = runMethod;
        }

        public string ShortCut { get; private set; }
        public string Title { get; private set; }
        public Func<string> RunMethod { get; set; }

        public bool Highlight = false;

        public void ChangeHighlight()
        {
            Highlight = Highlight == false;
        }

        public override string ToString()
        {
            var str = " " + Title;

            if (Highlight)
            {
                str += Parser.White + " <<< " + Parser.White;
            }

            return str;
        }
    }
}