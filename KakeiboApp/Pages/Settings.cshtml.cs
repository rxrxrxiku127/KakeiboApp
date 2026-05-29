using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    /// <summary>
    /// 設定画面のPageModel
    /// カテゴリ・カード・固定費の一覧をDBから取得して表示する
    /// </summary>
    public class SettingsModel : PageModel
    {
        // =====================================================
        // DBコンテキスト（DIで注入）
        // =====================================================
        private readonly KakeiboDbContext _db;

        /// <summary>コンストラクタ（DBコンテキストをDI）</summary>
        public SettingsModel(KakeiboDbContext db)
        {
            _db = db;
        }

        // =====================================================
        // 画面に渡すデータ
        // =====================================================

        /// <summary>カテゴリ一覧</summary>
        public List<Category> Categories { get; set; } = new();

        /// <summary>カード一覧</summary>
        public List<CreditCard> Cards { get; set; } = new();

        /// <summary>固定費一覧</summary>
        public List<FixedExpense> FixedExpenses { get; set; } = new();

        /// <summary>
        /// GETリクエスト処理
        /// DBから全データを取得する
        /// </summary>
        public async Task OnGetAsync()
        {
            // カテゴリ一覧をDBから取得
            Categories = await _db.Categories.ToListAsync();

            // カード一覧をDBから取得
            Cards = await _db.Cards.ToListAsync();

            // 固定費一覧をDBから取得
            FixedExpenses = await _db.FixedExpenses.ToListAsync();
        }

        /// <summary>
        /// カテゴリIDからカテゴリ名を取得する
        /// 固定費一覧でカテゴリ名を表示するために使用
        /// </summary>
        public string GetCategoryName(string id)
            => Categories.FirstOrDefault(c => c.Id == id)?.Name ?? "";
    }
}