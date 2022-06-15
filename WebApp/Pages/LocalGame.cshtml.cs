using BattleShipBrain;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parser = TcParser.TcParser;
using Helper = WebHelperMethods.WebHelperMethods;

namespace WebApp.Pages;

public class LocalGame : PageModel
{
    private GameConfig Config { get; set; } = null!;
    public BsBrain Brain { get; set; } = null!;
    public string GId { get; set; } = null!;
    
    private readonly BattleShipDbContext _context;

    public LocalGame(BattleShipDbContext context)
    {
        _context = context;
    }

    public IActionResult OnGet(bool newGame = false, bool againstAi = false, int x = -1, int y = -1, string? gId = null)
    {
        UpdateViewDataFlags();

        if (gId != null)
        {
            GId = gId;
            Brain = _context.LoadBsBrainBySaveName(gId);
            Brain.RestoreShipPlacementState(_context.LoadBrainShipPlacementStateByBrainName(gId));
        }

        if (newGame)
        {
            var newGId = InitBrain(againstAi);
            var routeValues = new Dictionary<string, string>
            {
                { "newGame", "true" },
                { "gId", newGId }
            };
            return RedirectToPage("ShipPlacement", routeValues);
        }
        
        if (Brain.IsGameAgainstAi() && Brain.GetCurrentPlayerId() == 2)
        {
            var aiMove = Brain.AiMakeMove();
            x = aiMove.X;
            y = aiMove.Y;
        }

        if (x <= -1 || y <= -1) return Page();
        var chosenSquareState = Brain.GetBoard(Brain.GetOpponentPlayerId())[x, y];

        if (chosenSquareState.IsBomb) return Page();
        Brain.Fire(x, y);

        if (Brain.HasCurrentPlayerWon())
        {
            AutoSave();
            ViewData["isEnded"] = true;
            return Page();
        }

        if (chosenSquareState.IsShip)
        {
            AutoSave();
            return Brain.IsGameAgainstAi() ? OnGet() : Page();
        }

        Brain.SwapCurrentPlayerId();
        ViewData["passMove"] = true;
        
        AutoSave();
        return Brain.IsGameAgainstAi() ? OnGet() : Page();
    }

    private string InitBrain(bool againstAi)
    {
        Config = Helper.GetStandardGameConfig();
        Brain = new BsBrain(Config);
        if (againstAi) Brain.SetGameAgainstAi();

        var uuid = Guid.NewGuid() + ". Autosave";
        Brain.InitShipPlacement();
        _context.SaveBsBrainToDb(Brain, Config, uuid);
        _context.SaveBrainShipPlacementStateByBrainName(uuid);

        return uuid;
    }

    private void UpdateViewDataFlags()
    {
        ViewData["isEnded"] = false;
        ViewData["saveGame"] = false;
        ViewData["saveGameDb"] = false;
        ViewData["saveGameJson"] = false;
    }
    
    private void AutoSave()
    {
        _context.UpdateExistingBrain(GId, Brain, false);
    }
}