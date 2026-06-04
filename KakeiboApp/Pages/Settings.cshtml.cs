using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    [Authorize]
    public class SettingsModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public SettingsModel(KakeiboDbContext db) { _db = db; }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public List<Category> Categories { get; set; } = new();
        public List<CreditCard> Cards { get; set; } = new();
        public List<FixedExpense> FixedExpenses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = GetUserId();
            Categories = await _db.Categories.Where(c => c.UserId == userId).ToListAsync();
            Cards = await _db.Cards.Where(c => c.UserId == userId).ToListAsync();
            FixedExpenses = await _db.FixedExpenses.Where(f => f.UserId == userId).ToListAsync();
        }

        public string GetCategoryName(string id)
            => Categories.FirstOrDefault(c => c.Id == id)?.Name ?? "";
    }
}