using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    [Authorize]
    public class BillingHistoryModel : PageModel
    {
        private readonly KakeiboDbContext _db;

        public BillingHistoryModel(KakeiboDbContext db) { _db = db; }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public int Year { get; set; }
        public int Month { get; set; }
        public List<Transaction> Transactions { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int TotalAmount { get; set; }

        public async Task OnGetAsync(
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            var userId = GetUserId();
            Year = year ?? DateTime.Today.Year;
            Month = month ?? DateTime.Today.Month;

            Categories = await _db.Categories
                .Where(c => c.UserId == userId).ToListAsync();

            // カード・QR・電子マネーの取引を翌月払いとして扱う
            var billingMethods = new[]
            {
                PaymentMethod.Card,
                PaymentMethod.QRPayment,
                PaymentMethod.ElectronicMoney
            };

            Transactions = await _db.Transactions
                .Where(t => t.UserId == userId &&
                            billingMethods.Contains(t.PaymentMethod) &&
                            t.Type == TransactionType.Expense &&
                            t.Date.Year == Year &&
                            t.Date.Month == Month)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            TotalAmount = Transactions.Sum(t => t.Amount);
        }

        public Category? GetCategory(string id)
            => Categories.FirstOrDefault(c => c.Id == id);

        public string GetPaymentLabel(PaymentMethod method) => method switch
        {
            PaymentMethod.Card => "💳 カード",
            PaymentMethod.QRPayment => "📱 QR決済",
            PaymentMethod.ElectronicMoney => "📲 電子マネー",
            _ => ""
        };
    }
}