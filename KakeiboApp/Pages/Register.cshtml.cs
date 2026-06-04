using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KakeiboApp.Pages
{
    /// <summary>
    /// 新規登録画面のPageModel
    /// </summary>
    public class RegisterModel : PageModel
    {
        public void OnGet()
        {
            // すでにログイン済みならホームにリダイレクト
            if (User.Identity?.IsAuthenticated == true)
            {
                Response.Redirect("/");
            }
        }
    }
}