using BattleShipBrain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Helper = WebHelperMethods.WebHelperMethods;

namespace WebApp.Pages;

public class Settings : PageModel
{
    public GameConfig Config = null!;
    public void OnGet(bool reset = false)
    {
        if (reset)
        {
            Helper.UpdateStandardConfig(new GameConfig());
            Config = Helper.GetStandardGameConfig();
            return;
        }

        Config = Helper.GetStandardGameConfig();
    }

    public static bool ValidateConfig(GameConfig gameConfig)
    {
        var testBrain = new BsBrain(gameConfig);
        var testResult = testBrain.ValidateCurrentGameSettings();

        return !(testResult < BsBrain.AmountOfBoardGenerationDuringSettingsValidation * 0.05);
    }
}