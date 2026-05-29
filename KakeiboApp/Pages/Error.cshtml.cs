using Microsoft.AspNetCore.Mvc.RazorPages;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    /// <summary>
    /// カテゴリ編集画面のPageModel
    /// </summary>
    public class EditCategoryModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public EditCategoryModel(KakeiboDbContext db)
        {
            _db = db;
        }

        /// <summary>編集対象のカテゴリ</summary>
        public Category? Category { get; set; }

        public async Task OnGetAsync(string id)
        {
            Category = await _db.Categories.FindAsync(id);
        }
    }
}