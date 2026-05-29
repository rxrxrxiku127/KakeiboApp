using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KakeiboApp.Models;
using System.Text.Json;

namespace KakeiboApp.Pages
{
    /// <summary>
    /// 取引編集画面のPageModel
    /// URLパラメータのIDで取引を取得してフォームに渡す
    /// </summary>
    public class EditModel : PageModel
    {
        // =====================================================
        // DBコンテキスト（DIで注入）
        // =====================================================
        private readonly KakeiboDbContext _db;

        /// <summary>コンストラクタ（DBコンテキストをDI）</summary>
        public EditModel(KakeiboDbContext db)
        {
            _db = db;
        }

        // =====================================================
        // 画面に渡すデータ
        // =====================================================

        /// <summary>編集対象の取引（nullの場合は取引が見つからなかった）</summary>
        public Transaction? Transaction { get; set; }

        /// <summary>カード一覧</summary>
        public List<CreditCard> Cards { get; set; } = new();

        /// <summary>カテゴリ一覧のJSON（JavaScriptに渡す）</summary>
        public string CategoriesJson { get; set; } = "[]";

        /// <summary>
        /// GETリクエスト処理
        /// URLパラメータのIDで取引を取得する
        /// </summary>
        public async Task OnGetAsync(string id)
        {
            // IDで取引を検索
            Transaction = await _db.Transactions.FindAsync(id);

            // カード一覧を取得
            Cards = await _db.Cards.ToListAsync();

            // カテゴリ一覧をJSONに変換
            var categories = await _db.Categories.ToListAsync();
            CategoriesJson = JsonSerializer.Serialize(
                categories.Select(c => new { id = c.Id, name = c.Name, icon = c.Icon })
            );
        }
    }
}