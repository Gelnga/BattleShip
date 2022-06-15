using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using BattleShipBrain;
using BattleShipConsoleUI;
using DAL;
using Domain;
using Parser = TcParser.TcParser;

namespace BattleShipConsole
{
    class Program
    {
        private static string _basePath = null!;
        private static string _pathToStandardConfig = null!;
        private static string _standardSavesLocation = null!;
        private static GameConfig _currentGameConfig = null!;
        private static BsBrain _currentBsBrain = null!;
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Clear();

            _basePath = args.Length == 1 ? args[0] : Directory.GetCurrentDirectory().Split("bin")[0];
            _currentGameConfig = new GameConfig();
            
            _pathToStandardConfig = _basePath + "Configs" + Path.DirectorySeparatorChar + "standard.json";
            _standardSavesLocation = _basePath + "Saves";

            if (!File.Exists(_pathToStandardConfig))
            {
                Console.WriteLine("Saving default config!");
                File.WriteAllText(_pathToStandardConfig, _currentGameConfig.ToString());
            }

            if (File.Exists(_pathToStandardConfig))
            {
                Console.WriteLine("Loading config...");
                var confText = File.ReadAllText(_pathToStandardConfig);
                _currentGameConfig = JsonSerializer.Deserialize<GameConfig>(confText)!;
            }

            _currentBsBrain = new BsBrain(_currentGameConfig);

            var mainMenu = new Menu("Main menu", EMenuLevel.Root, OutputMainMenuHints);
            mainMenu.AddMenuItems(new List<MenuItem>
            {
                new MenuItem("NG", "New game", NewGameMenu),
                new MenuItem("LG", "Load game", LoadGameMenu),
                new MenuItem("LGDB", "Load game from DB", LoadGameMenuDb),
                new MenuItem("S", "Settings", Settings)
            });
            
            mainMenu.Run();
        }

        private static void OutputMainMenuHints()
        {
            Parser.ParseColorAndPrint(Parser.LineFeed + " Use " + Parser.Magenta + "Up" + Parser.Magenta +
                                      " and" + Parser.Magenta + " Down " + Parser.Magenta + "arrow keys to navigate in menu." +
                                      " To choose an option, press" + Parser.White +" Enter " + Parser.White + 
                                      Parser.LineFeed + Parser.LineFeed);
        }

        private static string NewGameMenu()
        {
            _currentBsBrain = new BsBrain(_currentGameConfig);
            
            var newGame = new Menu("New game", EMenuLevel.First, null!);
            newGame.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("PvP", "Player versus Player", LaunchPvPGame),
                new MenuItem("PvE", "Player versus AI", LaunchPvEGame),
            });
            return newGame.Run();
        }

        private static string LaunchPvPGame()
        {
            var gameUi = new BsConsoleUi(_currentBsBrain);
            
            // Use for debug, comment out if statement
            // _currentBsBrain.GenerateShipPlacementOnCurrentBoard();
            // _currentBsBrain.GenerateShipPlacementOnCurrentBoard();
            
            if (!_currentBsBrain.AreShipsPlaced())
            {
                gameUi.LaunchTutorial();
                gameUi.PlaceShipsOnCurrentPlayerBoard();
                Console.Clear();
                Console.WriteLine("Pass controls to the next player. Next player - press any key to continue");
                Console.ReadKey();
                gameUi.PlaceShipsOnCurrentPlayerBoard();
            }

            var inGameMenu = GetInGameMenu(gameUi);

            return inGameMenu.Run();
        }
        
        private static string LaunchPvEGame()
        {
            var gameUi = new BsConsoleUi(_currentBsBrain);

            if (!_currentBsBrain.AreShipsPlaced())
            {
                gameUi.LaunchTutorial();
                gameUi.PlaceShipsOnCurrentPlayerBoard();
                _currentBsBrain.GenerateShipPlacementOnCurrentBoard();
                _currentBsBrain.SwapCurrentPlayerId();
                _currentBsBrain.SetGameAgainstAi();
            }

            var inGameMenu = GetInGameMenu(gameUi);

            return inGameMenu.Run();
        }

        private static Menu GetInGameMenu(BsConsoleUi gameUi)
        {
            var inGameMenu = new Menu("InGame menu", EMenuLevel.SecondOrMore, gameUi.DrawInGamePlayerView);
            inGameMenu.AddMenuItems(new List<MenuItem>
            {
                new MenuItem("M", "Make move", gameUi.MakeMove),
                new MenuItem("S", "Save game", SaveGameState),
                new MenuItem("SDB", "Save game to DB", SaveGameStateDb)
            });
            inGameMenu.MakeMenuCautious();
            return inGameMenu;
        }

        private static string SaveGameState()
        {
            var gameDto = _currentBsBrain.GetBrainJson();
            var saveName = AskUserToEnterGameSaveName() + ". " + DateTime.Now.ToString("dd.MM.yy hh:mm:ss");
            saveName = saveName.Replace(":", Parser.Col);
            
            SaveGameConfiguration(saveName, false);
            
            File.WriteAllText(_standardSavesLocation
                                        + Path.DirectorySeparatorChar + saveName + ".json", gameDto);
            
            Console.Clear();
            Console.WriteLine("Game was successfully saved! Press any key to continue");
            Console.ReadKey();
            return "";
        }

        private static string SaveGameStateDb()
        {

            var saveName = AskUserToEnterGameSaveName() + ". " + DateTime.Now.ToString("dd.MM.yy hh:mm:ss");

            using var db = new BattleShipDbContext();

            db.SaveBsBrainToDb(_currentBsBrain, _currentGameConfig, saveName);
            
            Console.Clear();
            Console.WriteLine("Game was successfully saved! Press any key to continue");
            Console.ReadKey();
        
            return "";
        }

        private static string LoadGameMenu()
        {
            var loadMenu = new Menu("Load game menu", EMenuLevel.First, PrintLoadingMenuHints);
            loadMenu.MakeMenuSupportDeletion();

            foreach (var save in Directory.GetFiles(_standardSavesLocation))
            {
                var splitSave = save.Split("Saves" + Path.DirectorySeparatorChar)[1]
                    .Split(".json")[0];
                splitSave = Parser.Decode(splitSave);
                var menuItem = new MenuItem(Guid.NewGuid().ToString(), splitSave, null!);
                menuItem.RunMethod = () => Parser.Encode(menuItem.Title);
                loadMenu.AddMenuItem(menuItem, true);
            }

            loadMenu.MenuItems.Sort((menuItem1, menuItem2) => ExtractDateTimeFromSaveAsMenuItem(menuItem2)
                .CompareTo(ExtractDateTimeFromSaveAsMenuItem(menuItem1)));

            var runOutput = loadMenu.Run();
            
            if (Menu.IsSpecialOutput(runOutput))
            {
                return runOutput;
            }
            
            if (runOutput.ToCharArray()[^1] == 'D')
            {
                var saveFile = runOutput[..^1];
                saveFile = Parser.Encode(saveFile);
                DeleteJsonSaveFile(saveFile);
                return LoadGameMenu();
            }

            var chosenSaveConfFile = _pathToStandardConfig.Split("standard")[0] + runOutput + ".json";
            
            var confText = File.ReadAllText(chosenSaveConfFile);
            _currentBsBrain = new BsBrain(JsonSerializer.Deserialize<GameConfig>(confText)!);
            
            var saveData = File.ReadAllText(_standardSavesLocation +
                                                      Path.DirectorySeparatorChar + runOutput + ".json");
            
            _currentBsBrain.RestoreBrainFromJson(JsonSerializer.Deserialize<SaveGameDto>(saveData)!);
            Console.Clear();
            LaunchPvPGame();

            return "";
        }

        private static string LoadGameMenuDb()
        {
            using var db = new BattleShipDbContext();

            var loadMenu = new Menu("Load game from database", EMenuLevel.First, PrintLoadingMenuHints);
            loadMenu.MakeMenuSupportDeletion();
            var savedBrains = db.SavedBsBrains;

            foreach (var save in savedBrains)
            {
                var menuItem = new MenuItem(Guid.NewGuid().ToString(), save.SavedBrainName, null!);
                menuItem.RunMethod = () => menuItem.Title;
                loadMenu.AddMenuItem(menuItem, true);
            }

            loadMenu.MenuItems.Sort((menuItem1, menuItem2) => ExtractDateTimeFromSaveAsMenuItem(menuItem2)
                .CompareTo(ExtractDateTimeFromSaveAsMenuItem(menuItem1)));

            var runOutput = loadMenu.Run();
            if (Menu.IsSpecialOutput(runOutput))
            {
                return runOutput;
            }
            
            if (runOutput.ToCharArray()[^1] == 'D')
            {
                var saveName = runOutput[..^1];
                db.DeleteBattleshipSaveFileById(db.SavedBsBrains
                    .First(savedBrain => savedBrain.SavedBrainName == saveName).Id);
                LoadGameMenuDb();
            }
            
            _currentBsBrain = db.LoadBsBrainBySaveName(runOutput);

            Console.Clear();
            LaunchPvPGame();
        
            return "";
        }

        private static string Settings()
        {
            var settings = new Menu("Settings", EMenuLevel.First, DisplayCurrentGameConfigs);
            settings.AddMenuItems(new List<MenuItem>()
            {
                new MenuItem("CGBS", "Configure game board size", GameBoardSizeConfiguration),
                new MenuItem("CGS", "Configure game ships", GameShipsConfiguration),
                new MenuItem("CTR", "Configure ships touch rule", TouchRuleConfiguration),
                new MenuItem("RTD", Parser.Red + "Reset to default settings" + Parser.Red, ResetSettingsToDefault)
            });
            return settings.Run();
        }

        private static void DisplayCurrentGameConfigs()
        {
            Console.WriteLine("Board size configuration");
            Console.WriteLine("Board width: " + _currentGameConfig.BoardWidth);
            Console.WriteLine("Board Length: " + _currentGameConfig.BoardLength);
            Console.WriteLine("-----------------------");
            Console.WriteLine("Ship configuration");
            foreach (var ship in _currentGameConfig.ShipConfigs)
            {
                Console.WriteLine(ship.Name + ": " + ship.Quantity);
            }
            Console.WriteLine();
            PrintCurrentGameTouchRule();
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
            Console.WriteLine("-----------------------");
        }
        
        private static string GameBoardSizeConfiguration()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Input width of the game board, it should be in a range of 1 to 26: ");
                var x = Console.ReadLine()?.Trim();

                if (!int.TryParse(x, out var xSize))
                {
                    Console.WriteLine("Wrong argument: " + x + ". Enter a number");
                } else if (xSize > 26)
                {
                    Console.WriteLine("Game board width can't be bigger than 26");
                } else if (xSize < 1)
                {
                    Console.WriteLine("Game board width can't be less than 1");
                } else
                {
                    _currentGameConfig.BoardWidth = xSize;
                    break;
                }
            }

            Console.Clear();
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Input length of the game board. It should be in a range of 1 to 40: ");
                var y = Console.ReadLine()?.Trim();
                
                if (!int.TryParse(y, out var ySize))
                {
                    Console.WriteLine("Wrong argument: " + y + ". Enter a number");
                } else if (ySize > 40)
                {
                    Console.WriteLine("Game board length can't be bigger than 40");
                } else if (ySize < 1)
                {
                    Console.WriteLine("Game board length can't be less than 1");
                } else
                {
                    _currentGameConfig.BoardLength = ySize;
                    break;
                }
            }
            Console.Clear();

            SaveGameConfiguration();
            return "";
        }
        
        private static string GameShipsConfiguration()
        {
            var shipMenus = new List<MenuItem>();
            foreach (var ship in _currentGameConfig.ShipConfigs)
            {
                shipMenus.Add(new MenuItem(ship.Name, ship.Name, () => ConfigureShip(ship.Name)));
            }
            
            var shipConfigurationMenu = new Menu("Ship configuration menu", EMenuLevel.SecondOrMore, null!);
            shipConfigurationMenu.AddMenuItems(shipMenus);
            return shipConfigurationMenu.Run();
        }

        private static string ConfigureShip(string shipName)
        {
            var reqShip = _currentGameConfig.ShipConfigs.FirstOrDefault(ship => ship.Name == shipName);
            var shipMenu = new Menu(shipName + " configuration menu", EMenuLevel.SecondOrMore,
                () => DisplayShipConfiguration(reqShip!));
            
            shipMenu.AddMenuItems(new List<MenuItem>(){
                new MenuItem("N", "Name", () => ConfigureShipName(reqShip!)),
                new MenuItem("Q", "Quantity", () => ConfigureShipQuantity(reqShip!)),
                new MenuItem("S", "Size", () => ConfigureShipSize(reqShip!))
            });
            return shipMenu.Run();
        }

        private static void DisplayShipConfiguration(ShipConfig reqShip)
        {
            Console.WriteLine("Name: " + reqShip.Name);
            Console.WriteLine("Quantity: " + reqShip.Quantity);
            Console.WriteLine("Ship width: " + reqShip.ShipSizeX);
            Console.WriteLine("Ship length: " + reqShip.ShipSizeY);
            Console.WriteLine("-----------------------");
        }

        private static string ConfigureShipName(ShipConfig reqShip)
        {
            Console.WriteLine("Input ship name: ");
            var name = Console.ReadLine()?.Trim();
            reqShip.Name = name!;
            Console.Clear();
            
            SaveGameConfiguration();
            return "";
        }
        
        private static string ConfigureShipQuantity(ShipConfig reqShip)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Input ship quantity, it shouldn't be less than 0: ");
                var quantity = Console.ReadLine()?.Trim();

                if (!int.TryParse(quantity, out var quantityParsed))
                {
                    continue;
                }

                if (quantityParsed < 0)
                {
                    continue;
                }
                
                reqShip.Quantity = quantityParsed;
                break;
            }

            SaveGameConfiguration();
            return "";
        }
        
        private static string ConfigureShipSize(ShipConfig reqShip)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Input ship width, it shouldn't be less than 1: ");
                var width = Console.ReadLine()?.Trim();
                if (!int.TryParse(width, out var widthParsed))
                {
                    continue;
                }

                if (widthParsed < 1)
                {
                    continue;
                }

                reqShip.ShipSizeX = widthParsed;
                break;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Input ship length, it shouldn't be less than 1: ");
                var length = Console.ReadLine()?.Trim();
                if (!int.TryParse(length, out var lengthParsed))
                {
                    continue;
                }

                if (lengthParsed < 1)
                {
                    continue;
                }

                reqShip.ShipSizeY = lengthParsed;
                break;
            }
            
            SaveGameConfiguration();
            return "";
        }

        private static string TouchRuleConfiguration()
        {
            var touchRulesMenu = new Menu("Ships touch rule configuration menu", EMenuLevel.SecondOrMore,
                PrintCurrentGameTouchRule);

            var menuItems = new List<MenuItem>();
            foreach (var touchRule in Enum.GetValues(typeof(EShipTouchRule)))
            {
                menuItems.Add(new MenuItem(touchRule.ToString()!, touchRule.ToString()!, 
                    () => SetTouchRule((EShipTouchRule) touchRule)));
            }
            
            touchRulesMenu.AddMenuItems(menuItems);
            return touchRulesMenu.Run();
        }

        private static void PrintCurrentGameTouchRule()
        {
            Parser.ParseColorAndPrint("Current touch rule: " + Parser.White + 
                                      _currentGameConfig.EShipTouchRule + Parser.White + Parser.LineFeed + Parser.LineFeed);
        }

        private static string SetTouchRule(EShipTouchRule touchRule)
        {
            _currentGameConfig.EShipTouchRule = touchRule;
            SaveGameConfiguration();
            
            return "";
        }

        private static void SaveGameConfiguration(string saveName = "", bool giveWarning = true)
        {
            var saveLocation = _pathToStandardConfig;
            if (saveName != "")
            {
                saveLocation = _pathToStandardConfig.Split("standard")[0] + saveName +
                               _pathToStandardConfig.Split("standard")[1];
            }

            if (giveWarning)
            {
                var testBrain = new BsBrain(_currentGameConfig);
                var rotations = BsBrain.AmountOfBoardGenerationDuringSettingsValidation;
                var successfulRotations = testBrain.ValidateCurrentGameSettings();
                
                if (successfulRotations == 0)
                {
                    Parser.ParseColorAndPrint(Parser.Red + "Warning! " + Parser.Red + 
                                              "With current game configuration ships can't fit on a game board! " +
                                              "Settings didn't change" + Parser.LineFeed + Parser.LineFeed);
                    
                    DisplaySettingsHint();
                    
                    var confText = File.ReadAllText(_pathToStandardConfig);
                    _currentGameConfig = JsonSerializer.Deserialize<GameConfig>(confText)!;
                    return;
                }
                
                if (rotations - rotations * 0.9 >= successfulRotations) {
                    Parser.ParseColorAndPrint(Parser.Red + "Warning! " + Parser.Red + 
                                              "With current game configuration there is a high chance that during ship " +
                                              Parser.LineFeed +
                                              "placement phase there will be a situation, in which all ships can't fit on" +
                                              " a game board! ONLY AUTOMATIC BOARD GENERATION IS AVAILABLE NOW!" + 
                                              Parser.LineFeed + Parser.LineFeed);
                    
                    DisplaySettingsHint();
                    
                } else if (rotations - rotations / 2 >= successfulRotations) {
                    
                    Parser.ParseColorAndPrint(Parser.Red + "Warning! " + Parser.Red + 
                                              "With current game configuration there is a decent chance that during ship " +
                                              Parser.LineFeed +
                                              "placement phase there will be a situation, in which all ships can't fit on" +
                                              " a game board! Board automatic generation is recommended" + Parser.LineFeed +
                                              Parser.LineFeed);
                    
                    DisplaySettingsHint();
                }
            }

            var currentGameConfigJson = JsonSerializer.Serialize(_currentGameConfig);
            File.WriteAllText(saveLocation, currentGameConfigJson);
        }
        
        private static string ResetSettingsToDefault()
        {
            var answer = CommonUiParts.AreYouSureMenu("reset all settings to default");
            if (!answer) return "";
            
            var defaultConfig = new GameConfig();
            _currentGameConfig = defaultConfig;
            File.WriteAllText(_pathToStandardConfig, defaultConfig.ToString());

            return "";
        }

        private static void DisplaySettingsHint()
        {
            if (_currentGameConfig.EShipTouchRule == EShipTouchRule.NoTouch)
            {
                Parser.ParseColorAndPrint(Parser.Magenta + "Hint: " + Parser.Magenta +
                                          "to increase number of ships that can fit on game board" +
                                          " you can try changing ships touch rule to side touch or corner touch" +
                                          Parser.LineFeed + Parser.LineFeed);
            }

            // If pressed key is hold the warning message can be unintentionally skipped
            Thread.Sleep(50);
            Console.WriteLine("Press any key to go back to settings");
            Console.ReadKey();
        }

        private static string AskUserToEnterGameSaveName()
        {
            string gameName;
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Enter saved game name and then press Enter. Save name shouldn't be empty!: ");
                var input = Console.ReadLine()?.Trim();
                gameName = input ?? "";
                if (gameName != "")
                {
                    break;
                }
            }

            return gameName;
        }

        private static DateTime ExtractDateTimeFromSaveAsMenuItem(MenuItem save)
        {
            var mi1DateStr = save.Title.Split(". ")[^1];
            mi1DateStr = mi1DateStr.Replace(Parser.Col, ":");
            return Convert.ToDateTime(mi1DateStr);
        }

        private static void DeleteJsonSaveFile(string saveName)
        {
            File.Delete(_standardSavesLocation + Path.DirectorySeparatorChar + saveName + ".json");
            File.Delete(_pathToStandardConfig.Split("standard")[0] + saveName +
                        _pathToStandardConfig.Split("standard")[1]);
        }

        private static void PrintLoadingMenuHints()
        {
            Parser.ParseColorAndPrint(Parser.LineFeed + 
                                      Parser.White + " You can delete any highlighted save file by pressing " +
                                      Parser.White +
                                      Parser.Magenta +
                                      "D " +
                                      Parser.Magenta +
                                      Parser.White +
                                      "key" + Parser.White +
                                      Parser.LineFeed + Parser.LineFeed);
            Console.WriteLine("-----------------------");
        }
    }
}