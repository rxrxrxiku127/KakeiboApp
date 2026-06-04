using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public AddModel(KakeiboDbContext db) { _db = db; }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public List<CreditCard> Cards { get; set; } = new();
        public string CategoriesJson { get; set; } = "[]";

        public async Task OnGetAsync()
        {
            var userId = GetUserId();
            Cards = await _db.Cards.Where(c => c.UserId == userId).ToListAsync();
            var categories = await _db.Categories.Where(c => c.UserId == userId).ToListAsync();
            CategoriesJson = JsonSerializer.Serialize(
                categories.Select(c => new { id = c.Id, name = c.Name, icon = c.Icon }));
        }
    }
}