using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class SaveGame : PageModel
{
    public string GId = null!;
    public void OnGet(string gId)
    {
        GId = gId;
    }
}