using System.Text.Json;
using BattleShipBrain;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parser = TcParser.TcParser;
using Helper = WebHelperMethods.WebHelperMethods;

namespace WebApp.Pages;

public class SaveGameJson : PageModel
{
    private readonly BattleShipDbContext _context;
    
    public string GId = null!;
    
    private BsBrain _brain = null!;
    private GameConfig _config = null!;

    public SaveGameJson(BattleShipDbContext context)
    {
        _context = context;
    }

    public void OnGet(string gId)
    {
        GId = gId;
    }

    public IActionResult OnPost(string? saveNameJson = null, string? gId = null)
    {
        if (saveNameJson == null || gId == null) return Page();
        GId = gId;
        
        _brain = _context.LoadBsBrainBySaveName(GId);
        _config = _context.LoadBsBrainConfigByBsBrainSaveName(GId);
        
        var saveName = saveNameJson + ". " + DateTime.Now.ToString("dd.MM.yy hh:mm:ss");
        saveName = saveName.Replace(":", Parser.Col);
            
        var gameDto = _brain.GetBrainJson();
        var currentGameConfigJson = JsonSerializer.Serialize(_config);

        var saveLocation = Helper.GetSaveLocationPathBySaveName(saveName);
        var confLocation = Helper.GetConfLocationPathBySaveName(saveName);
            
        System.IO.File.WriteAllText(saveLocation, gameDto);
        System.IO.File.WriteAllText(confLocation, currentGameConfigJson);

        var routeValues = new Dictionary<string, string>
        {
            { "gId", GId }
        };
        return RedirectToPage("LocalGame", routeValues);
    }
}