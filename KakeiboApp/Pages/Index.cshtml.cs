using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KakeiboApp.Models;

namespace KakeiboApp.Pages
{
    /// <summary>
    /// 予算進捗表示用のデータクラス
    /// カテゴリごとの使用額・上限・使用率を保持する
    /// </summary>
    public class BudgetProgressItem
    {
        /// <summary>カテゴリのアイコン（絵文字）</summary>
        public string Icon { get; set; } = "";

        /// <summary>カテゴリ名</summary>
        public string Name { get; set; } = "";

        /// <summary>今月の使用額（円）</summary>
        public int Used { get; set; }

        /// <summary>予算上限（円）</summary>
        public int Limit { get; set; }

        /// <summary>
        /// 使用率（%）
        /// Used / Limit * 100 で計算（上限0の場合は0）
        /// </summary>
        public int Percent => Limit > 0 ? Used * 100 / Limit : 0;
    }

    /// <summary>
    /// ホーム画面のPageModel
    /// 今月の収支サマリー・予算進捗・取引一覧・通知を表示する
    /// </summary>
    public class IndexModel : PageModel
    {
        // =====================================================
        // DBコンテキスト（DIで注入）
        // =====================================================
        private readonly KakeiboDbContext _db;

        /// <summary>コンストラクタ（DBコンテキストをDI）</summary>
        public IndexModel(KakeiboDbContext db)
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

        /// <summary>今月の取引一覧</summary>
        public List<Transaction> Transactions { get; set; } = new();

        /// <summary>予算通知メッセージ一覧</summary>
        public List<string> Notifications { get; set; } = new();

        /// <summary>カテゴリ別予算進捗</summary>
        public List<BudgetProgressItem> BudgetProgress { get; set; } = new();

        /// <summary>今月の収入合計</summary>
        public int TotalIncome { get; set; }

        /// <summary>今月の支出合計</summary>
        public int TotalExpense { get; set; }

        /// <summary>残高（収入 - 支出）</summary>
        public int Balance => TotalIncome - TotalExpense;

        /// <summary>全カテゴリ（取引一覧でカテゴリ名を逆引きするために使用）</summary>
        public List<Category> Categories { get; set; } = new();

        /// <summary>
        /// GETリクエスト処理
        /// URLパラメータから年月を取得して今月のデータを表示する
        /// </summary>
        public async Task OnGetAsync(int? year, int? month)
        {
            // 年月が指定されていない場合は今月を表示
            Year = year ?? DateTime.Today.Year;
            Month = month ?? DateTime.Today.Month;

            // =====================================================
            // 全カテゴリを取得（取引一覧でのカテゴリ名逆引き用）
            // =====================================================
            Categories = await _db.Categories.ToListAsync();

            // =====================================================
            // 今月の取引を取得（日付降順で表示）
            // =====================================================
            Transactions = await _db.Transactions
                .Where(t => t.Date.Year == Year && t.Date.Month == Month)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // =====================================================
            // 収入・支出の合計を計算
            // =====================================================
            TotalIncome = Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            TotalExpense = Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            // =====================================================
            // カテゴリ別予算進捗を計算
            // 予算上限が設定されているカテゴリのみ表示
            // =====================================================
            BudgetProgress = Categories
                .Where(c => c.BudgetLimit > 0)
                .Select(c => new BudgetProgressItem
                {
                    Icon = c.Icon,
                    Name = c.Name,
                    Used = Transactions
                            .Where(t => t.CategoryId == c.Id && t.Type == TransactionType.Expense)
                            .Sum(t => t.Amount),
                    Limit = c.BudgetLimit
                }).ToList();

            // =====================================================
            // 予算通知を生成
            // - 100%以上：予算超過の警告
            // - 80%以上：予算残り少の注意
            // =====================================================
            foreach (var b in BudgetProgress)
            {
                if (b.Percent >= 100)
                    Notifications.Add($"{b.Icon} {b.Name}の予算{b.Limit:#,0}円を超えました！（現在：{b.Used:#,0}円）");
                else if (b.Percent >= 80)
                    Notifications.Add($"{b.Icon} {b.Name}の予算まで残り{(b.Limit - b.Used):#,0}円です");
            }
        }

        // =====================================================
        // ヘルパーメソッド
        // =====================================================

        /// <summary>
        /// カテゴリIDからカテゴリを取得する
        /// 取引一覧でカテゴリ名・アイコンを表示するために使用
        /// </summary>
        public Category? GetCategory(string id)
            => Categories.FirstOrDefault(c => c.Id == id);

        /// <summary>
        /// 支払い方法のenumを日本語ラベルに変換する
        /// </summary>
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