using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KakeiboApp.Models;
using System.Text.Json;

namespace KakeiboApp.Pages
{
    /// <summary>
    /// レポート画面のPageModel
    /// 月次の収支サマリー・カテゴリ別・支払い方法別の集計を行う
    /// </summary>
    public class ReportModel : PageModel
    {
        // =====================================================
        // DBコンテキスト（DIで注入）
        // =====================================================
        private readonly KakeiboDbContext _db;

        /// <summary>コンストラクタ（DBコンテキストをDI）</summary>
        public ReportModel(KakeiboDbContext db)
        {
            _db = db;
        }

        // =====================================================
        // 画面に渡すデータ
        // =====================================================

        /// <summary>表示中の年</summary>
        public int Year { get; set; }

        /// <summary>表示中の月</summary>
        public int Month { get; set; }

        /// <summary>今月の収入合計</summary>
        public int TotalIncome { get; set; }

        /// <summary>今月の支出合計</summary>
        public int TotalExpense { get; set; }

        /// <summary>残高（収入 - 支出）</summary>
        public int Balance => TotalIncome - TotalExpense;

        /// <summary>カテゴリ別予算進捗</summary>
        public List<BudgetProgressItem> BudgetItems { get; set; } = new();

        /// <summary>円グラフ用JSON（カテゴリ別支出）</summary>
        public string PieChartJson { get; set; } = "[]";

        /// <summary>棒グラフ用JSON（支払い方法別支出）</summary>
        public string PayChartJson { get; set; } = "[]";

        /// <summary>
        /// GETリクエスト処理
        /// URLパラメータから年月を取得して集計する
        /// </summary>
        public async Task OnGetAsync(int? year, int? month)
        {
            // 年月が指定されていない場合は今月
            Year = year ?? DateTime.Today.Year;
            Month = month ?? DateTime.Today.Month;

            // =====================================================
            // 今月の取引を取得
            // =====================================================
            var transactions = await _db.Transactions
                .Where(t => t.Date.Year == Year && t.Date.Month == Month)
                .ToListAsync();

            // 収入・支出の合計
            TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            TotalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            // =====================================================
            // カテゴリ別集計
            // =====================================================
            var categories = await _db.Categories.ToListAsync();

            BudgetItems = categories.Select(c => new BudgetProgressItem
            {
                Icon = c.Icon,
                Name = c.Name,
                Used = transactions
                        .Where(t => t.CategoryId == c.Id && t.Type == TransactionType.Expense)
                        .Sum(t => t.Amount),
                Limit = c.BudgetLimit
            })
            .Where(b => b.Used > 0) // 支出がある場合のみ表示
            .ToList();

            // 円グラフ用JSONを生成
            PieChartJson = JsonSerializer.Serialize(
                BudgetItems.Select(b => new { name = b.Icon + b.Name, amount = b.Used })
            );

            // =====================================================
            // 支払い方法別集計
            // =====================================================
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