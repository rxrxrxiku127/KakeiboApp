using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    [Authorize]
    public class BudgetProgressItem
    {
        public string Icon { get; set; } = "";
        public string Name { get; set; } = "";
        public int Used { get; set; }
        public int Limit { get; set; }
        public int Percent => Limit > 0 ? Used * 100 / Limit : 0;
    }

    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public IndexModel(KakeiboDbContext db) { _db = db; }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public int Year { get; set; }
        public int Month { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public const int PageSize = 8;
        public List<Transaction> Transactions { get; set; } = new();
        public List<Transaction> AllTransactions { get; set; } = new();
        public List<string> Notifications { get; set; } = new();
        public List<BudgetProgressItem> BudgetProgress { get; set; } = new();
        public int TotalIncome { get; set; }
        public int TotalExpense { get; set; }
        public int Balance => TotalIncome - TotalExpense;
        public List<Category> Categories { get; set; } = new();
        public string DisplayName { get; set; } = "";

        public async Task OnGetAsync(
            [FromQuery] int? year,
            [FromQuery] int? month,
            [FromQuery] int? page)
        {
            var userId = GetUserId();
            Year = year ?? DateTime.Today.Year;
            Month = month ?? DateTime.Today.Month;
            CurrentPage = page ?? 1;
            DisplayName = User.FindFirstValue(ClaimTypes.Name) ?? "";

            Categories = await _db.Categories
                .Where(c => c.UserId == userId).ToListAsync();

            AllTransactions = await _db.Transactions
                .Where(t => t.UserId == userId &&
                            t.Date.Year == Year &&
                            t.Date.Month == Month)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            TotalIncome = AllTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            TotalExpense = AllTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            TotalPages = (int)Math.Ceiling(AllTransactions.Count / (double)PageSize);
            if (TotalPages == 0) TotalPages = 1;
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            Transactions = AllTransactions
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            BudgetProgress = Categories
                .Where(c => c.BudgetLimit > 0)
                .Select(c => new BudgetProgressItem
                {
                    Icon = c.Icon,
                    Name = c.Name,
                    Used = AllTransactions
                            .Where(t => t.CategoryId == c.Id && t.Type == TransactionType.Expense)
                            .Sum(t => t.Amount),
                    Limit = c.BudgetLimit
                }).ToList();

            foreach (var b in BudgetProgress)
            {
                if (b.Percent >= 100)
                    Notifications.Add($"{b.Icon} {b.Name}の予算{b.Limit:#,0}円を超えました！（現在：{b.Used:#,0}円）");
                else if (b.Percent >= 80)
                    Notifications.Add($"{b.Icon} {b.Name}の予算まで残り{(b.Limit - b.Used):#,0}円です");
            }
        }

        public Category? GetCategory(string id)
            => Categories.FirstOrDefault(c => c.Id == id);

        public string GetPaymentLabel(PaymentMethod method) => method switch
        {
            PaymentMethod.Cash => "現金",
            PaymentMethod.Card => "カード",
            PaymentMethod.ElectronicMoney => "電子マネー",
            PaymentMethod.QRPayment => "QR決済",
            PaymentMethod.BankAccount => "口座",
            _ => ""
        };
    }
}