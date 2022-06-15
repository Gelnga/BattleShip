using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class LoadGame : PageModel
{ public IActionResult OnGet()
     {
         return Page();
     }
}
