using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    [Authorize]
    public class ReportModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public ReportModel(KakeiboDbContext db) { _db = db; }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalIncome { get; set; }
        public int TotalExpense { get; set; }
        public int Balance => TotalIncome - TotalExpense;
        public List<BudgetProgressItem> BudgetItems { get; set; } = new();
        public string PieChartJson { get; set; } = "[]";
        public string PayChartJson { get; set; } = "[]";

        public async Task OnGetAsync(int? year, int? month)
        {
            var userId = GetUserId();
            Year = year ?? DateTime.Today.Year;
            Month = month ?? DateTime.Today.Month;

            var transactions = await _db.Transactions
                .Where(t => t.UserId == userId &&
                            t.Date.Year == Year &&
                            t.Date.Month == Month)
                .ToListAsync();

            TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            TotalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            var categories = await _db.Categories
                .Where(c => c.UserId == userId).ToListAsync();

            BudgetItems = categories.Select(c => new BudgetProgressItem
            {
                Icon = c.Icon,
                Name = c.Name,
                Used = transactions
                        .Where(t => t.CategoryId == c.Id && t.Type == TransactionType.Expense)
                        .Sum(t => t.Amount),
                Limit = c.BudgetLimit
            }).Where(b => b.Used > 0).ToList();

            PieChartJson = JsonSerializer.Serialize(
                BudgetItems.Select(b => new { name = b.Icon + b.Name, amount = b.Used }));

            var payGroups = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.PaymentMethod)
                .Select(g => new {
                    name = g.Key switch
                    {
                        PaymentMethod.Cash => "現金",
                        PaymentMethod.Card => "カード",
                        PaymentMethod.ElectronicMoney => "電子マネー",
                        PaymentMethod.QRPayment => "QR決済",
                        PaymentMethod.BankAccount => "口座",
                        _ => ""
                    },
                    amount = g.Sum(t => t.Amount)
                });

            PayChartJson = JsonSerializer.Serialize(payGroups);
        }
    }
}