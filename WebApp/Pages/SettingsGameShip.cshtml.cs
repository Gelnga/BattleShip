using BattleShipBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Helper = WebHelperMethods.WebHelperMethods;

namespace WebApp.Pages;

public class SettingsGameShip : PageModel
{
    public ShipConfig ShipConfig = null!;
    public GameConfig Config = Helper.GetStandardGameConfig();
    public int ShipConfId;

    public IActionResult OnPost(string? name = null, int? width = null, int? length = null, int? quantity = null, int? shipConfId = null)
    {
        if (shipConfId == null)
        {
            throw new Exception("Ship configuration id should be passed in order to configure a certain ship");
        }

        var routeValues = new Dictionary<string, string>
        {
            { "shipConfId", shipConfId.Value.ToString() }
        };
        
        if (name == null || width == null || length == null || quantity == null) return RedirectToPage("SettingsGameShip", routeValues);
        
        if (width.Value < 1)
        {
            Console.WriteLine("check");
            return RedirectToPage("SettingsGameShip", routeValues);
        }

        if (length.Value < 1)
        {
            return RedirectToPage("SettingsGameShip", routeValues);
        }

        if (quantity.Value < 0)
        {
            return RedirectToPage("SettingsGameShip", routeValues);
        }

        ShipConfig = Config.ShipConfigs[shipConfId.Value];
        ShipConfig.Name = name;
        ShipConfig.ShipSizeX = width.Value;
        ShipConfig.ShipSizeY = length.Value;
        ShipConfig.Quantity = quantity.Value;

        if (!Settings.ValidateConfig(Config)) return RedirectToPage("SettingsGameShip", routeValues);
        
        Helper.UpdateStandardConfig(Config);
        return RedirectToPage("Settings");

    }

    public void OnGet(int? shipConfId = null)
    {
        if (shipConfId != null)
        {
            ShipConfId = shipConfId.Value;
            ShipConfig = Config.ShipConfigs[ShipConfId];
        }
    }
}