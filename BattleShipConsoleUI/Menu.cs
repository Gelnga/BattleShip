using System;
using System.Collections.Generic;
using System.Linq;
using Parser = TcParser.TcParser;

namespace BattleShipConsoleUI
{
    public class Menu
    {
        public readonly List<MenuItem> MenuItems = new();
        private readonly List<MenuItem> _specialMenuItems = new();
        private readonly MenuItem _menuItemExit = new("E", Parser.Red + "Exit" + Parser.Red,
            () => AreYouSureMenuItem(true, "exit")!);
        private readonly MenuItem _menuItemBack = new("B", "Back", 
            () => AreYouSureMenuItem(_cautiousMenu, "go back")!);
        private readonly MenuItem _menuItemHome = new("MM", "Back to main menu", 
            () => AreYouSureMenuItem(_cautiousMenu, "go back to main menu")!);

        private static bool _cautiousMenu;
        private bool _supportsDeletion;

        private readonly HashSet<string> _menuShortCuts = new();
        private readonly HashSet<string> _menuSpecialShortCuts = new()
        {
            "C",
            "D"
        };

        private int _highlightedMenuShortcut;

        private readonly string _title;
        private readonly EMenuLevel _menuLevel;
        private readonly Action _customMenuPart;
        
        public Menu(string title , EMenuLevel menuLevel, Action customMenuPart)
        {
            _title = title;
            _menuLevel = menuLevel;
            _customMenuPart = customMenuPart;
            _cautiousMenu = false;
            
            switch (_menuLevel)
            {
                case EMenuLevel.Root:
                    AddMenuItem(_menuItemExit, _specialMenuItems);
                    break;
                case EMenuLevel.First:
                    AddMenuItem(_menuItemHome, _specialMenuItems);
                    AddMenuItem(_menuItemExit, _specialMenuItems);
                    break;
                case EMenuLevel.SecondOrMore:
                    AddMenuItem(_menuItemBack, _specialMenuItems);
                    AddMenuItem(_menuItemHome, _specialMenuItems);
                    AddMenuItem(_menuItemExit, _specialMenuItems);
                    break;
            }
        }

        public void AddMenuItem(MenuItem item, bool isActionEndingMenuRun = false)
        {
            AddMenuItem(item, MenuItems, isActionEndingMenuRun);
        }

        private void AddMenuItem(MenuItem item, List<MenuItem> itemList, bool isActionEndingMenuRun = true, int position = -1)
        {
            if (_menuShortCuts.Add(item.ShortCut.ToUpper()) == false)
            {
                throw new ApplicationException($"Conflicting menu shortcut {item.ShortCut.ToUpper()}");
            }

            if (isActionEndingMenuRun)
            {
                _menuSpecialShortCuts.Add(item.ShortCut.ToUpper());
            }

            if (position == -1)
            {
                itemList.Add(item);
            }
            else
            {
                itemList.Insert(position, item);
            }
        }
        
        public void AddMenuItems(List<MenuItem> items, List<MenuItem> itemList = null!, bool isActionEndingMenuRun = false)
        {
            itemList = itemList == null! ? MenuItems : itemList;

            foreach (var menuItem in items)
            {
                AddMenuItem(menuItem, itemList, isActionEndingMenuRun);
            }
        }

        // private void DeleteMenuItem(MenuItem menuItem)
        // {
        //     if (_specialMenuItems.Contains(menuItem)) return;
        //     MenuItems.Remove(menuItem);
        //     _menuShortCuts.Remove(menuItem.ShortCut);
        // }

        public string Run()
        {
            var runDone = false;
            string input;
            do
            {
                input = OutputMenu().ToUpper();
                
                if (input == "D")
                {
                    var highlightedItem = MenuItems[_highlightedMenuShortcut];
                    var itemToDeleteTitle = highlightedItem.Title + "D";
                    
                    return itemToDeleteTitle;
                }

                Console.Clear();
                var isInputValid = _menuShortCuts.Contains(input);
                if (isInputValid)
                {
                    var item = MenuItems.FirstOrDefault(t => t.ShortCut.ToUpper() == input) ??
                               _specialMenuItems.FirstOrDefault(t => t.ShortCut.ToUpper() == input);

                    var methodOutput = item!.RunMethod();
                    input = methodOutput == null! ? item.ShortCut : methodOutput;
                    
                    if (input == "C") continue;
                    
                    runDone = _menuSpecialShortCuts.Contains(item.ShortCut.ToUpper()) || 
                              _menuSpecialShortCuts.Contains(input);
                }

                if (!runDone && !isInputValid)
                {
                    Console.WriteLine($"Unknown shortcut '{input}'!");
                }

            } while (!runDone);
            
            if (input == _menuItemExit.ShortCut.ToUpper()) Environment.Exit(0);
            if (input == _menuItemHome.ShortCut.ToUpper() && _menuLevel == EMenuLevel.SecondOrMore) return "MM";
            if (input == _menuItemBack.ShortCut.ToUpper() || input == _menuItemHome.ShortCut.ToUpper()) return "";
            return input;
        }

        private string OutputMenu()
        {
            List<MenuItem> menuItems = new();
            menuItems.AddRange(MenuItems);
            menuItems.AddRange(_specialMenuItems);
            
            ConsoleKeyInfo keyInput;
            
            do
            {
                Console.Clear();
                menuItems[_highlightedMenuShortcut].ChangeHighlight();

                Console.WriteLine("\n====> " + _title + " <====");
                Console.WriteLine("-----------------------");

                _customMenuPart?.Invoke();

                foreach (var menuItem in MenuItems)
                {
                    Parser.ParseColorAndPrint(menuItem.ToString());
                    Console.WriteLine();
                }

                Console.WriteLine("-----------------------");

                foreach (var specialMenuItem in _specialMenuItems)
                {
                    Parser.ParseColorAndPrint(specialMenuItem.ToString());
                    Console.WriteLine();
                }

                Console.WriteLine("=======================");
                
                menuItems[_highlightedMenuShortcut].ChangeHighlight();
                
                keyInput = Console.ReadKey();
                switch (keyInput.Key)
                {
                    case ConsoleKey.DownArrow:
                        _highlightedMenuShortcut++;
                        break;
                    case ConsoleKey.UpArrow:
                        _highlightedMenuShortcut--;
                        break;
                    case ConsoleKey.D:
                        if (!_supportsDeletion) break;
                        if (!CommonUiParts.AreYouSureMenu(
                                "delete save file " + MenuItems[_highlightedMenuShortcut].Title
                                )) break;
                        
                        return "D";
                    case ConsoleKey.Enter:
                        return menuItems[_highlightedMenuShortcut].ShortCut;
                    default:
                        continue;
                }

                if (_highlightedMenuShortcut < 0)
                {
                    _highlightedMenuShortcut += menuItems.Count;
                }

                if (_highlightedMenuShortcut > menuItems.Count - 1)
                {
                    _highlightedMenuShortcut -= menuItems.Count;
                }
                
            } while (keyInput.Key != ConsoleKey.Escape);

            Environment.Exit(0);
            return "";
        }

        public void MakeMenuCautious()
        {
            _cautiousMenu = true;
        }

        public void MakeMenuSupportDeletion()
        {
            _supportsDeletion = true;
        }

        public static bool IsSpecialOutput(string output)
        {
            var specialOutputs = new HashSet<String>
            {
                "",
                "MM"
            };

            return specialOutputs.Contains(output);
        }

        private static string? AreYouSureMenuItem(bool isCautious, string message)
        {
            if (!isCautious) return null;

            var answer = CommonUiParts.AreYouSureMenu(message);

            return !answer ? "C" : null;
        }
    }
}