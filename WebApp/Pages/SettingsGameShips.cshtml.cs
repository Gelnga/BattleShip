using BattleShipBrain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Helper = WebHelperMethods.WebHelperMethods;

namespace WebApp.Pages;

public class SettingsGameShips : PageModel
{
    public GameConfig Config = Helper.GetStandardGameConfig();
    public void OnGet()
    {
        
    }
}