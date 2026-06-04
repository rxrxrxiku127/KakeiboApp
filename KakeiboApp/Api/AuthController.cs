using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KakeiboApp.Models;

namespace KakeiboApp.Api
{
    /// <summary>
    /// 認証APIコントローラー
    /// ルート: /api/auth
    /// ユーザー登録・ログイン・ログアウトを担当する
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly KakeiboDbContext _db;

        public AuthController(KakeiboDbContext db)
        {
            _db = db;
        }

        // =====================================================
        // 新規登録
        // POST /api/auth/register
        // =====================================================

        /// <summary>
        /// ユーザーを新規登録する
        /// パスワードはSHA256でハッシュ化して保存する
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            // 入力チェック
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password) ||
                string.IsNullOrWhiteSpace(req.DisplayName))
            {
                return BadRequest(new { message = "全項目を入力してください" });
            }

            // パスワードの長さチェック
            if (req.Password.Length < 6)
            {
                return BadRequest(new { message = "パスワードは6文字以上にしてください" });
            }

            // メールアドレスの重複チェック
            var exists = await _db.Users.AnyAsync(u => u.Email == req.Email);
            if (exists)
            {
                return BadRequest(new { message = "このメールアドレスはすでに登録されています" });
            }

            // ユーザーを作成
            var user = new AppUser
            {
                Email = req.Email.Trim().ToLower(),
                PasswordHash = HashPassword(req.Password),
                DisplayName = req.DisplayName.Trim()
            };

            _db.Users.Add(user);

            // =====================================================
            // デフォルトカテゴリを作成
            // 新規ユーザー登録時にデフォルトカテゴリを自動で追加する
            // =====================================================
            var defaultCategories = new List<Category>
            {
                new Category { UserId = user.Id, Name = "食費",   Icon = "🍚", BudgetLimit = 50000, IsDefault = true },
                new Category { UserId = user.Id, Name = "交通費", Icon = "🚃", BudgetLimit = 10000, IsDefault = true },
                new Category { UserId = user.Id, Name = "日用品", Icon = "🧴", BudgetLimit = 15000, IsDefault = true },
                new Category { UserId = user.Id, Name = "光熱費", Icon = "💡", BudgetLimit = 20000, IsDefault = true },
                new Category { UserId = user.Id, Name = "娯楽",   Icon = "🎮", BudgetLimit = 20000, IsDefault = true },
                new Category { UserId = user.Id, Name = "医療費", Icon = "🏥", BudgetLimit = 10000, IsDefault = true },
                new Category { UserId = user.Id, Name = "衣服",   Icon = "👕", BudgetLimit = 15000, IsDefault = true },
                new Category { UserId = user.Id, Name = "通信費", Icon = "📱", BudgetLimit = 10000, IsDefault = true },
                new Category { UserId = user.Id, Name = "給料",   Icon = "💴", BudgetLimit = 0,     IsDefault = true },
                new Category { UserId = user.Id, Name = "その他", Icon = "📦", BudgetLimit = 10000, IsDefault = true },
            };

            _db.Categories.AddRange(defaultCategories);
            await _db.SaveChangesAsync();

            return Ok(new { message = "登録しました！" });
        }

        // =====================================================
        // ログイン
        // POST /api/auth/login
        // =====================================================

        /// <summary>
        /// ログインする
        /// 認証成功時にCookieを発行する
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            // 入力チェック
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new { message = "メールアドレスとパスワードを入力してください" });
            }

            // ユーザーを検索
            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.Email == req.Email.Trim().ToLower());

            // ユーザーが存在しない or パスワードが違う
            if (user == null || user.PasswordHash != HashPassword(req.Password))
            {
                return Unauthorized(new { message = "メールアドレスまたはパスワードが違います" });
            }

            // =====================================================
            // Cookie認証のクレームを設定
            // ユーザーIDと表示名をCookieに保存する
            // =====================================================
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name,           user.DisplayName),
                new Claim(ClaimTypes.Email,          user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Cookieを発行してログイン状態にする
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    // ブラウザを閉じてもCookieを保持する
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                });

            return Ok(new { message = "ログインしました！" });
        }

        // =====================================================
        // ログアウト
        // POST /api/auth/logout
        // =====================================================

        /// <summary>
        /// ログアウトする
        /// Cookieを削除してログイン画面にリダイレクトする
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Cookieを削除
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "ログアウトしました" });
        }

        // =====================================================
        // パスワードハッシュ化（SHA256）
        // =====================================================

        /// <summary>
        /// パスワードをSHA256でハッシュ化する
        /// </summary>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }

    // =====================================================
    // リクエストモデル
    // =====================================================

    /// <summary>新規登録リクエスト</summary>
    public class RegisterRequest
    {
        /// <summary>表示名</summary>
        public string DisplayName { get; set; } = "";

        /// <summary>メールアドレス</summary>
        public string Email { get; set; } = "";

        /// <summary>パスワード</summary>
        public string Password { get; set; } = "";
    }

    /// <summary>ログインリクエスト</summary>
    public class LoginRequest
    {
        /// <summary>メールアドレス</summary>
        public string Email { get; set; } = "";

        /// <summary>パスワード</summary>
        public string Password { get; set; } = "";
    }
}