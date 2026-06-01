using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    public class EditCategoryModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public EditCategoryModel(KakeiboDbContext db)
        {
            _db = db;
        }

        public Category? Category { get; set; }

        public async Task OnGetAsync(string id)
        {
            Category = await _db.Categories.FindAsync(id);
        }
    }
}
