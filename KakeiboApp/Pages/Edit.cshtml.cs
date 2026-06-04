using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public EditModel(KakeiboDbContext db) { _db = db; }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public Transaction? Transaction { get; set; }
        public List<CreditCard> Cards { get; set; } = new();
        public string CategoriesJson { get; set; } = "[]";

        public async Task OnGetAsync(string id)
        {
            var userId = GetUserId();
            Transaction = await _db.Transactions.FirstOrDefaultAsync(
                t => t.Id == id && t.UserId == userId);
            Cards = await _db.Cards.Where(c => c.UserId == userId).ToListAsync();
            var categories = await _db.Categories.Where(c => c.UserId == userId).ToListAsync();
            CategoriesJson = JsonSerializer.Serialize(
                categories.Select(c => new { id = c.Id, name = c.Name, icon = c.Icon }));
        }
    }
}