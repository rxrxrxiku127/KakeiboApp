using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using KakeiboApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// =====================================================
// SQLite データベースの設定
// =====================================================
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                       ?? "Data Source=kakeibo.db";

builder.Services.AddDbContext<KakeiboDbContext>(options =>
    options.UseSqlite(connectionString));

// =====================================================
// Cookie認証の設定
// ログイン・ログアウトをCookieで管理する
// =====================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // 未ログイン時のリダイレクト先
        options.LoginPath = "/Login";
        // ログアウト後のリダイレクト先
        options.LogoutPath = "/Logout";
        // Cookieの有効期限（30日）
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        // スライディングセッション（アクセスのたびに期限を延長）
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// =====================================================
// データベースの自動マイグレーション
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KakeiboDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 認証・認可ミドルウェア（必ずUseRoutingの後に）
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();