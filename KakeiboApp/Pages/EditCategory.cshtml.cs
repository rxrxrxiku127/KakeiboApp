using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    [Authorize]
    public class EditCategoryModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public EditCategoryModel(KakeiboDbContext db) { _db = db; }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public Category? Category { get; set; }

        public async Task OnGetAsync(string id)
        {
            var userId = GetUserId();
            Category = await _db.Categories.FirstOrDefaultAsync(
                c => c.Id == id && c.UserId == userId);
        }
    }
}