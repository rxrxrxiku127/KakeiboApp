using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KakeiboApp.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        public void OnGet() { }
    }
}