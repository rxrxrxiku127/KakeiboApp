using Microsoft.EntityFrameworkCore;
using KakeiboApp.Models;

// =====================================================
// サービスの登録
// DIコンテナに各サービスを登録する
// =====================================================
var builder = WebApplication.CreateBuilder(args);

// Razor Pages を使用する
builder.Services.AddRazorPages();

// Web API コントローラーを使用する
builder.Services.AddControllers();

// =====================================================
// SQLite データベースの設定
// kakeibo.db というファイルにデータを保存する
// ファイルはアプリと同じフォルダに作成される
// =====================================================
builder.Services.AddDbContext<KakeiboDbContext>(options =>
    options.UseSqlite("Data Source=kakeibo.db"));

var app = builder.Build();

// =====================================================
// データベースの自動マイグレーション
// アプリ起動時にDBが存在しない場合は自動で作成する
// 初期データ（デフォルトカテゴリ）も自動で挿入される
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KakeiboDbContext>();

    // DBが存在しない場合は作成・マイグレーションを実行
    db.Database.EnsureCreated();
}

// =====================================================
// ミドルウェアの設定
// リクエストの処理順序を定義する
// =====================================================

// HTTPS にリダイレクト
app.UseHttpsRedirection();

// wwwroot フォルダの静的ファイルを配信
app.UseStaticFiles();

// ルーティングを有効化
app.UseRouting();

// Razor Pages のルートを登録
app.MapRazorPages();

// API コントローラーのルートを登録
app.MapControllers();

app.Run();