using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    [Authorize]
    public class EditFixedModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public EditFixedModel(KakeiboDbContext db) { _db = db; }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public FixedExpense? Fixed { get; set; }
        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync(string id)
        {
            var userId = GetUserId();
            Fixed = await _db.FixedExpenses.FirstOrDefaultAsync(
                f => f.Id == id && f.UserId == userId);
            Categories = await _db.Categories
                .Where(c => c.UserId == userId).ToListAsync();
        }
    }
}