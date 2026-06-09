using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    [Authorize]
    public class CalendarModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public CalendarModel(KakeiboDbContext db) { _db = db; }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public int Year { get; set; }
        public int Month { get; set; }
        public List<Category> Categories { get; set; } = new();
        public Dictionary<int, (int Income, int Expense)> DailyTotals { get; set; } = new();

        public async Task OnGetAsync(
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            var userId = GetUserId();
            Year = year ?? DateTime.Today.Year;
            Month = month ?? DateTime.Today.Month;

            Categories = await _db.Categories
                .Where(c => c.UserId == userId).ToListAsync();

            var transactions = await _db.Transactions
                .Where(t => t.UserId == userId &&
                            t.Date.Year == Year &&
                            t.Date.Month == Month)
                .ToListAsync();

            DailyTotals = transactions
                .GroupBy(t => t.Date.Day)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Income: g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                        Expense: g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                    )
                );
        }
    }
}