using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Helper = WebHelperMethods.WebHelperMethods;

namespace WebApp.Pages;

public class LoadGameDb : PageModel
{
    private readonly BattleShipDbContext _context;
    public List<SavedBsBrain> SavesList { get; set; } = default!;
    public LoadGameDb(BattleShipDbContext context)
    {
        _context = context;
    }

    public IActionResult OnPost(int? id = null)
    {
        if (id != null)
        {
            _context.DeleteBattleshipSaveFileById(id.Value);
        }
         
        UpdateSavesList();
        return Page();
    }

    public IActionResult OnGet(int? id = null)
    {
        if (id != null)
        {
            var loadedBrain = _context.LoadBsBrainById(id.Value);
            var loadedConf = _context.LoadBsBrainConfigByBsBrainId(id.Value);
            
            var gId = Guid.NewGuid() + ". Autosave";
            _context.SaveBsBrainToDb(loadedBrain, loadedConf, gId);
            _context.SaveBrainShipPlacementStateByBrainName(gId);
            
            var routeValues = new Dictionary<string, string>
            {
                { "gId", gId }
            };
            
            return RedirectToPage("LocalGame", routeValues);
        }
        
        UpdateSavesList();
        return Page();
    }

    private void UpdateSavesList()
    {
        SavesList = _context.SavedBsBrains.ToList();
        SavesList = SavesList.Where(save => save.SavedBrainName.Split(". ")[^1] != "Autosave").ToList();
        SavesList.Sort((brain1, brain2) => Helper.ExtractDateTimeFromSaveName(brain2.SavedBrainName)
            .CompareTo(Helper.ExtractDateTimeFromSaveName(brain1.SavedBrainName)));
    }
}