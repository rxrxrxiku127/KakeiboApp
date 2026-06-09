using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KakeiboApp.Models;

namespace KakeiboApp.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly KakeiboDbContext _db;

        public AuthController(KakeiboDbContext db) { _db = db; }

        // =====================================================
        // 新規登録 POST /api/auth/register
        // =====================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.UserId) ||
                string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "全項目を入力してください" });

            if (req.Password.Length < 6)
                return BadRequest(new { message = "パスワードは6文字以上にしてください" });

            var exists = await _db.Users.AnyAsync(u => u.UserId == req.UserId);
            if (exists)
                return BadRequest(new { message = "このユーザーIDはすでに使われています" });

            var user = new AppUser
            {
                UserId = req.UserId.Trim(),
                PasswordHash = HashPassword(req.Password)
            };

            _db.Users.Add(user);

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
        // ログイン POST /api/auth/login
        // =====================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.UserId) ||
                string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "ユーザーIDとパスワードを入力してください" });

            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.UserId == req.UserId.Trim());

            if (user == null || user.PasswordHash != HashPassword(req.Password))
                return Unauthorized(new { message = "ユーザーIDまたはパスワードが違います" });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name,           user.UserId)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                });

            return Ok(new { message = "ログインしました！" });
        }

        // =====================================================
        // ログアウト POST /api/auth/logout
        // =====================================================
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "ログアウトしました" });
        }

        // =====================================================
        // パスワード変更 POST /api/auth/change-password
        // =====================================================
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            if (user.PasswordHash != HashPassword(req.CurrentPassword))
                return BadRequest(new { message = "現在のパスワードが違います" });

            if (req.NewPassword.Length < 6)
                return BadRequest(new { message = "新しいパスワードは6文字以上にしてください" });

            user.PasswordHash = HashPassword(req.NewPassword);
            await _db.SaveChangesAsync();

            return Ok(new { message = "パスワードを変更しました！" });
        }

        // =====================================================
        // パスワードハッシュ化（SHA256）
        // =====================================================
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }

    public class RegisterRequest
    {
        public string UserId { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginRequest
    {
        public string UserId { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}