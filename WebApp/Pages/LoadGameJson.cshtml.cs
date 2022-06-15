using System.Text.Json;
using System.Web;
using BattleShipBrain;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parser = TcParser.TcParser;
using Helper = WebHelperMethods.WebHelperMethods;

namespace WebApp.Pages;

public class LoadGameJson : PageModel
{
    public List<string> SavesList = new();
    
    private readonly BattleShipDbContext _context;

    public LoadGameJson(BattleShipDbContext context)
    {
        _context = context;
    }

    public IActionResult OnPostAsync(string? saveName = null)
    {
        if (saveName != null)
        {
            saveName = HttpUtility.UrlDecode(saveName);
            System.IO.File.Delete(Helper.GetSaveLocationPathBySaveName(saveName));
            System.IO.File.Delete(Helper.GetConfigurationFolderPath() + Path.DirectorySeparatorChar + saveName);
        }
        
        UpdateSavesList();
        return Page();
    }

    public IActionResult OnGetAsync(string? saveName = null)
    {
        UpdateSavesList();
        
        if (saveName != null)
        {
            saveName = HttpUtility.UrlDecode(saveName);

            var confText = System.IO.File.ReadAllText(Helper.GetConfLocationPathBySaveName(saveName));
            var saveData = System.IO.File.ReadAllText(Helper.GetSaveLocationPathBySaveName(saveName));

            var loadedConf = JsonSerializer.Deserialize<GameConfig>(confText)!;
            var loadedBrain = new BsBrain(loadedConf);
            loadedBrain.RestoreBrainFromJson(JsonSerializer.Deserialize<SaveGameDto>(saveData)!);

            var gId = Guid.NewGuid() + ". Autosave";
            _context.SaveBsBrainToDb(loadedBrain, loadedConf, gId);
            _context.SaveBrainShipPlacementStateByBrainName(gId);
            
            var routeValues = new Dictionary<string, string>
            {
                { "gId", gId }
            };
            
            return RedirectToPage("LocalGame", routeValues);
        }
        
        return Page();
    }

    private void UpdateSavesList()
    {
        var savesFolderPath = Helper.GetSavesFolderPath();
        foreach (var save in Directory.GetFiles(savesFolderPath))
        {
            SavesList.Add(save.Replace(savesFolderPath + Path.DirectorySeparatorChar, ""));
        }
        
        SavesList.Sort((save1, save2) => 
            Helper.ExtractDateTimeFromSaveName(Parser.Decode(save2).Split(".json")[0])
            .CompareTo(Helper.ExtractDateTimeFromSaveName(Parser.Decode(save1).Split(".json")[0])));
    }
}