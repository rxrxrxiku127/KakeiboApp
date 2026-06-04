using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KakeiboApp.Pages
{
    /// <summary>
    /// ログイン画面のPageModel
    /// </summary>
    public class LoginModel : PageModel
    {
        /// <summary>エラーメッセージ（ログイン失敗時に表示）</summary>
        public string ErrorMessage { get; set; } = "";

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