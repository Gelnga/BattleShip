using BattleShipBrain;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class ShipPlacement : PageModel
{
    public const string UndoCommand = "undo";
    public const string RedoCommand = "redo";
    public const string PlaceCommand = "place";
    public const string GenerateCommand = "generate";

    private readonly BattleShipDbContext _context;
    public BsBrain Brain = null!;
    public string GId = null!;

    public ShipPlacement(BattleShipDbContext context)
    {
        _context = context;
    }

    public IActionResult OnGet(string? gId = null, int? rotationChange = null, string? cmd = null, int? x = null, int? y = null)
    {
        if (gId != null)
        {
            GId = gId;
            Brain = _context.LoadBsBrainBySaveName(gId);
            Brain.RestoreShipPlacementState(_context.LoadBrainShipPlacementStateByBrainName(gId));
        }
        else
        {
            throw new Exception(
                "Ship placement should happen on a loaded board from database. Game id was not provided");
        }

        if (x != null && y != null)
        {
            Brain.UpdateShipPlacementPreviewCoordinate(ValidateCoordinate(x.Value, y.Value));
            
            AutoSave();
            return Page();
        }
        
        if (rotationChange != null)
        {
            Brain.RotateShip(rotationChange.Value);
            
            AutoSave();
            return Page();
        }

        if (cmd == null) return Page();
        {
            switch (cmd)
            {
                case UndoCommand:
                    Brain.UndoLastShipPlacementOnCurrentBoard();
                    
                    break;
                
                case RedoCommand:
                    Brain.RedoLastShipPlacementOnCurrentBoard();

                    break;
                
                case PlaceCommand:
                    Brain.PlaceShip();

                    break;
                
                case GenerateCommand:
                    Brain.GenerateShipPlacementOnCurrentBoard();
                    
                    break;
            }
        }

        return CheckShipPlacementState();
    }

    private void AutoSave()
    {
        _context.UpdateExistingBrain(GId, Brain, true);
    }

    private Coordinate ValidateCoordinate(int x, int y)
    {
        if (!Brain.ValidateShipPlacementInsideGameBoardBoarders(new Coordinate(x, y))) 
            return Brain.GetShipPlacementPreviewCoordinate();

        var maxXDim = Brain.GetBoard(1).GetLength(0) - 1;
        var maxYDim = Brain.GetBoard(1).GetLength(1) - 1;

        x = ValidateCordDim(x, maxXDim);
        y = ValidateCordDim(y, maxYDim);

        return new Coordinate(x, y);
    }

    private int ValidateCordDim(int cordValue, int maxCordValue)
    {
        if (cordValue < 0)
        {
            return 0;
        }

        if (cordValue > maxCordValue)
        {
            return maxCordValue;
        }

        return cordValue;
    }

    private IActionResult CheckShipPlacementState()
    {
        if (Brain.AreShipsPlaced())
        {
            Brain.SwapCurrentPlayerId();
            
            AutoSave();
            var routeValues = new Dictionary<string, string>
            {
                { "gId", GId }
            };
            return RedirectToPage("LocalGame", routeValues);
        }

        if (Brain.AreShipsPlacedOnCurrentBoard() && !Brain.AreShipsPlaced())
        {
            Brain.SwapCurrentPlayerId(); 
            
            if (Brain.IsGameAgainstAi())
            {
                Brain.GenerateShipPlacementOnCurrentBoard();
                Brain.SwapCurrentPlayerId();
                
                AutoSave();
                var routeValues = new Dictionary<string, string>
                {
                    { "gId", GId }
                };
                return RedirectToPage("LocalGame", routeValues);
            }
           
            ViewData["passMove"] = true;
        }

        AutoSave();
        return Page();
    }
}