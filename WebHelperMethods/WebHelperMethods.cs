using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using BattleShipBrain;

namespace WebHelperMethods;

public static class WebHelperMethods
{
    public static string GetSquareStateClass(BoardSquareState squareState, bool isSunken)
    {
        if (isSunken)
        {
            return "sunken-ship";
        }

        return (squareState.IsShip, squareState.IsBomb) switch
        {
            (false, false) => "",
            (false, true) => "miss",
            (true, false) => "ship",
            (true, true) => "hit"
        };
    }

    public static string GetConfigurationFolderPath()
    {
        var rootProjectPath = Directory.GetParent(
            Directory.GetCurrentDirectory()
                        .Split("bin")[0])!.ToString();

        return rootProjectPath + Path.DirectorySeparatorChar + "BattleShipConsole" +
               Path.DirectorySeparatorChar + "Configs";
    }

    public static string GetConfLocationPathBySaveName(string saveName)
    {
        return GetConfigurationFolderPath() + Path.DirectorySeparatorChar + saveName;
    }

    public static string GetSavesFolderPath()
    {
        var rootProjectPath = Directory.GetParent(
            Directory.GetCurrentDirectory()
                .Split("bin")[0])!.ToString();

        return rootProjectPath + Path.DirectorySeparatorChar + "BattleShipConsole" +
               Path.DirectorySeparatorChar + "Saves";
    }

    public static string GetSaveLocationPathBySaveName(string saveName)
    {
        return GetSavesFolderPath() + Path.DirectorySeparatorChar + saveName;
    }
    
    public static DateTime ExtractDateTimeFromSaveName(string saveName)
    {
        var mi1DateStr = saveName.Split(". ")[^1];
        var format = "dd.MM.yy HH:mm:ss";
        
        return DateTime.ParseExact(mi1DateStr, format, CultureInfo.InvariantCulture);
    }

    public static GameConfig GetStandardGameConfig()
    {
        var config = new GameConfig();

        var pathToStandardConfig = GetStandardConfPath();
        
        if (!File.Exists(pathToStandardConfig))
        {
            UpdateStandardConfig(config);
        }
        
        if (File.Exists(pathToStandardConfig))
        {
            var confText = File.ReadAllText(pathToStandardConfig);
            config = JsonSerializer.Deserialize<GameConfig>(confText)!;
        }

        return config;
    }

    public static void UpdateStandardConfig(GameConfig config)
    {
        var confPath = GetStandardConfPath();
        File.WriteAllText(confPath, config.ToString());
    }

    public static string GetStandardConfPath()
    {
        var confPath = GetConfigurationFolderPath();
        var pathToStandardConfig = confPath + Path.DirectorySeparatorChar + "standard.json";
        return pathToStandardConfig;
    }
}