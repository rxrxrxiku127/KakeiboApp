using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KakeiboApp.Models;
using System.Text.Json;

namespace KakeiboApp.Pages
{
    /// <summary>
    /// 収支追加画面のPageModel
    /// カテゴリ一覧・カード一覧をDBから取得してフォームに渡す
    /// </summary>
    public class AddModel : PageModel
    {
        // =====================================================
        // DBコンテキスト（DIで注入）
        // =====================================================
        private readonly KakeiboDbContext _db;

        /// <summary>コンストラクタ（DBコンテキストをDI）</summary>
        public AddModel(KakeiboDbContext db)
        {
            _db = db;
        }

        // =====================================================
        // 画面に渡すデータ
        // =====================================================

        /// <summary>カード一覧（カード払い選択時に表示）</summary>
        public List<CreditCard> Cards { get; set; } = new();

        /// <summary>
        /// カテゴリ一覧のJSON文字列
        /// JavaScriptでカテゴリグリッドを動的に生成するために使用
        /// </summary>
        public string CategoriesJson { get; set; } = "[]";

        /// <summary>
        /// GETリクエスト処理
        /// DBからカテゴリ・カード一覧を取得してフォームに渡す
        /// </summary>
        public async Task OnGetAsync()
        {
            // カード一覧をDBから取得
            Cards = await _db.Cards.ToListAsync();

            // カテゴリ一覧をJSON形式に変換してJavaScriptに渡す
            // id・name・iconのみを渡す（最小限のデータ）
            var categories = await _db.Categories.ToListAsync();
            CategoriesJson = JsonSerializer.Serialize(
                categories.Select(c => new { id = c.Id, name = c.Name, icon = c.Icon })
            );
        }
    }
}