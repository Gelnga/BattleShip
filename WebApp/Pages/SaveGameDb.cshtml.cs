using BattleShipBrain;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class SaveGameDb : PageModel
{
    private readonly BattleShipDbContext _context;

    public string GId = null!;
    
    private BsBrain _brain = null!;
    private GameConfig _config = null!;
    public SaveGameDb(BattleShipDbContext context)
    {
        _context = context; 
    }
    
    public void OnGet(string gId)
    {
        GId = gId;
    }

    public IActionResult OnPost(string? gId = null, string? saveNameDb = null)
    {
        if (saveNameDb != null && gId != null)
        {
            GId = gId;
            
            _brain = _context.LoadBsBrainBySaveName(GId);
            _config = _context.LoadBsBrainConfigByBsBrainSaveName(GId);
            
            saveNameDb = saveNameDb + ". " + DateTime.Now.ToString("dd.MM.yy hh:mm:ss");
            _context.SaveBsBrainToDb(_brain, _config, saveNameDb);
            
            var routeValues = new Dictionary<string, string>
            {
                { "gId", GId }
            };
            return RedirectToPage("LocalGame", routeValues);
        }

        return Page();
    }
}