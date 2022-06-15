using BattleShipBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Helper = WebHelperMethods.WebHelperMethods;

namespace WebApp.Pages;

public class SettingsBoardSize : PageModel
{
    public GameConfig Config = Helper.GetStandardGameConfig();
    public IActionResult OnPost(int? boardWidth = null, int? boardLength = null)
    {
        if (boardWidth != null && boardLength != null)
        {
            Config.BoardWidth = boardWidth.Value;
            Config.BoardLength = boardLength.Value;
            
            if (boardWidth.Value <= 20 && boardLength.Value <= 15 &&
                boardWidth.Value >= 1 && boardLength.Value >= 1 &&
                Settings.ValidateConfig(Config))
            {
                Helper.UpdateStandardConfig(Config);
                return RedirectToPage("Settings");
            }
        }

        return Page();
    }
    
    public void OnGet()
    {
        
    }
}