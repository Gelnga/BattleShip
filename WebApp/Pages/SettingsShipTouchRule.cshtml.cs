using BattleShipBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Helper = WebHelperMethods.WebHelperMethods;

namespace WebApp.Pages;

public class SettingsShipTouchRule : PageModel
{
    public GameConfig Config = Helper.GetStandardGameConfig();
    public IActionResult OnPost(EShipTouchRule? touchRule = null)
    {
        if (touchRule != null)
        {
            Config.EShipTouchRule = touchRule.Value;
            
            if (Settings.ValidateConfig(Config))
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