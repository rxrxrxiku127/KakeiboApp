using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KakeiboApp.Models;

namespace KakeiboApp.Api
{
    /// <summary>
    /// 家計簿APIコントローラー
    /// ルート: /api/kakeibo
    /// SQLiteを使ってデータを永続化する
    /// </summary>
    [ApiController]
    [Route("api/kakeibo")]
    public class KakeiboController : ControllerBase
    {
        // =====================================================
        // DBコンテキスト
        // DIコンテナから自動で注入される
        // =====================================================
        private readonly KakeiboDbContext _db;

        /// <summary>コンストラクタ（DBコンテキストをDI）</summary>
        public KakeiboController(KakeiboDbContext db)
        {
            _db = db;
        }

        // ============================================================
        //  取引（Transaction）
        // ============================================================

        /// <summary>
        /// 取引を追加する
        /// POST /api/kakeibo/transaction
        /// </summary>
        [HttpPost("transaction")]
        public async Task<IActionResult> AddTransaction([FromBody] AddTransactionRequest req)
        {
            // リクエストからTransactionオブジェクトを生成
            var transaction = new Transaction
            {
                Amount = req.Amount,
                Date = DateTime.Parse(req.Date),
                Note = req.Note ?? "",
                CategoryId = req.CategoryId,
                PaymentMethod = Enum.Parse<PaymentMethod>(req.PaymentMethod),
                Type = Enum.Parse<TransactionType>(req.Type),
                IsFixed = req.IsFixed,
                CardId = req.CardId,
                BillingDate = string.IsNullOrEmpty(req.BillingDate)
                                ? null
                                : DateTime.Parse(req.BillingDate)
            };

            // DBに保存
            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            // 予算チェックを実行
            await CheckBudgetAsync(transaction.Date.Year, transaction.Date.Month);

            return Ok(new { message = "登録しました！", id = transaction.Id });
        }

        /// <summary>
        /// 取引を更新する
        /// PUT /api/kakeibo/transaction/{id}
        /// </summary>
        [HttpPut("transaction/{id}")]
        public async Task<IActionResult> UpdateTransaction(string id, [FromBody] AddTransactionRequest req)
        {
            // IDで取引を検索
            var item = await _db.Transactions.FindAsync(id);
            if (item == null) return NotFound();

            // 各フィールドを更新
            item.Amount = req.Amount;
            item.Date = DateTime.Parse(req.Date);
            item.Note = req.Note ?? "";
            item.CategoryId = req.CategoryId;
            item.PaymentMethod = Enum.Parse<PaymentMethod>(req.PaymentMethod);
            item.Type = Enum.Parse<TransactionType>(req.Type);
            item.IsFixed = req.IsFixed;
            item.CardId = req.CardId;
            item.BillingDate = string.IsNullOrEmpty(req.BillingDate)
                                 ? null
                                 : DateTime.Parse(req.BillingDate);

            // DBに保存
            await _db.SaveChangesAsync();

            return Ok(new { message = "更新しました！" });
        }

        /// <summary>
        /// 取引を削除する
        /// DELETE /api/kakeibo/transaction/{id}
        /// </summary>
        [HttpDelete("transaction/{id}")]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            var item = await _db.Transactions.FindAsync(id);
            if (item == null) return NotFound();

            _db.Transactions.Remove(item);
            await _db.SaveChangesAsync();

            return Ok(new { message = "削除しました！" });
        }

        // ============================================================
        //  カテゴリ（Category）
        // ============================================================

        /// <summary>
        /// カテゴリを追加する
        /// POST /api/kakeibo/category
        /// </summary>
        [HttpPost("category")]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest req)
        {
            var category = new Category
            {
                Name = req.Name,
                Icon = req.Icon,
                BudgetLimit = req.BudgetLimit,
                IsDefault = false // ユーザー追加カテゴリはデフォルトではない
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return Ok(new { message = "登録しました！", id = category.Id });
        }

        /// <summary>
        /// カテゴリを更新する
        /// PUT /api/kakeibo/category/{id}
        /// </summary>
        [HttpPut("category/{id}")]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] AddCategoryRequest req)
        {
            var item = await _db.Categories.FindAsync(id);
            if (item == null) return NotFound();

            item.Name = req.Name;
            item.Icon = req.Icon;
            item.BudgetLimit = req.BudgetLimit;

            await _db.SaveChangesAsync();

            return Ok(new { message = "更新しました！" });
        }

        /// <summary>
        /// カテゴリを削除する
        /// DELETE /api/kakeibo/category/{id}
        /// デフォルトカテゴリは削除不可
        /// </summary>
        [HttpDelete("category/{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var item = await _db.Categories.FindAsync(id);
            if (item == null) return NotFound();

            // デフォルトカテゴリは削除不可
            if (item.IsDefault) return BadRequest(new { message = "デフォルトカテゴリは削除できません" });

            _db.Categories.Remove(item);
            await _db.SaveChangesAsync();

            return Ok(new { message = "削除しました！" });
        }

        // ============================================================
        //  クレジットカード（CreditCard）
        // ============================================================

        /// <summary>
        /// カードを追加する
        /// POST /api/kakeibo/card
        /// </summary>
        [HttpPost("card")]
        public async Task<IActionResult> AddCard([FromBody] AddCardRequest req)
        {
            var card = new CreditCard
            {
                Name = req.Name,
                ClosingDay = req.ClosingDay,
                BillingDay = req.BillingDay
            };

            _db.Cards.Add(card);
            await _db.SaveChangesAsync();

            return Ok(new { message = "登録しました！", id = card.Id });
        }

        /// <summary>
        /// カードを更新する
        /// PUT /api/kakeibo/card/{id}
        /// </summary>
        [HttpPut("card/{id}")]
        public async Task<IActionResult> UpdateCard(string id, [FromBody] AddCardRequest req)
        {
            var item = await _db.Cards.FindAsync(id);
            if (item == null) return NotFound();

            item.Name = req.Name;
            item.ClosingDay = req.ClosingDay;
            item.BillingDay = req.BillingDay;

            await _db.SaveChangesAsync();

            return Ok(new { message = "更新しました！" });
        }

        /// <summary>
        /// カードを削除する
        /// DELETE /api/kakeibo/card/{id}
        /// </summary>
        [HttpDelete("card/{id}")]
        public async Task<IActionResult> DeleteCard(string id)
        {
            var item = await _db.Cards.FindAsync(id);
            if (item == null) return NotFound();

            _db.Cards.Remove(item);
            await _db.SaveChangesAsync();

            return Ok(new { message = "削除しました！" });
        }

        // ============================================================
        //  固定費（FixedExpense）
        // ============================================================

        /// <summary>
        /// 固定費を追加する
        /// POST /api/kakeibo/fixed
        /// </summary>
        [HttpPost("fixed")]
        public async Task<IActionResult> AddFixed([FromBody] AddFixedRequest req)
        {
            var fixed_ = new FixedExpense
            {
                Name = req.Name,
                Amount = req.Amount,
                DayOfMonth = req.DayOfMonth,
                CategoryId = req.CategoryId,
                PaymentMethod = Enum.Parse<PaymentMethod>(req.PaymentMethod)
            };

            _db.FixedExpenses.Add(fixed_);
            await _db.SaveChangesAsync();

            return Ok(new { message = "登録しました！", id = fixed_.Id });
        }

        /// <summary>
        /// 固定費を更新する
        /// PUT /api/kakeibo/fixed/{id}
        /// </summary>
        [HttpPut("fixed/{id}")]
        public async Task<IActionResult> UpdateFixed(string id, [FromBody] AddFixedRequest req)
        {
            var item = await _db.FixedExpenses.FindAsync(id);
            if (item == null) return NotFound();

            item.Name = req.Name;
            item.Amount = req.Amount;
            item.DayOfMonth = req.DayOfMonth;
            item.CategoryId = req.CategoryId;
            item.PaymentMethod = Enum.Parse<PaymentMethod>(req.PaymentMethod);

            await _db.SaveChangesAsync();

            return Ok(new { message = "更新しました！" });
        }

        /// <summary>
        /// 固定費を削除する
        /// DELETE /api/kakeibo/fixed/{id}
        /// </summary>
        [HttpDelete("fixed/{id}")]
        public async Task<IActionResult> DeleteFixed(string id)
        {
            var item = await _db.FixedExpenses.FindAsync(id);
            if (item == null) return NotFound();

            _db.FixedExpenses.Remove(item);
            await _db.SaveChangesAsync();

            return Ok(new { message = "削除しました！" });
        }

        // ============================================================
        //  固定費の自動計上
        // ============================================================

        /// <summary>
        /// 固定費を今月分として自動登録する
        /// POST /api/kakeibo/apply-fixed
        /// 同じ月に同じ固定費が既に登録されている場合はスキップ
        /// </summary>
        [HttpPost("apply-fixed")]
        public async Task<IActionResult> ApplyFixed([FromBody] YearMonthRequest req)
        {
            var fixedList = await _db.FixedExpenses.ToListAsync();

            foreach (var f in fixedList)
            {
                // 既に今月分が登録されているかチェック
                var exists = await _db.Transactions.AnyAsync(t =>
                    t.IsFixed &&
                    t.Note == f.Name &&
                    t.Date.Year == req.Year &&
                    t.Date.Month == req.Month);

                if (!exists)
                {
                    // 計上日（月末を超えないように調整）
                    var day = Math.Min(f.DayOfMonth, DateTime.DaysInMonth(req.Year, req.Month));
                    var date = new DateTime(req.Year, req.Month, day);

                    _db.Transactions.Add(new Transaction
                    {
                        Date = date,
                        Amount = f.Amount,
                        CategoryId = f.CategoryId,
                        PaymentMethod = f.PaymentMethod,
                        Type = TransactionType.Expense,
                        IsFixed = true,
                        Note = f.Name
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ============================================================
        //  予算チェック（内部メソッド）
        // ============================================================

        /// <summary>
        /// 予算チェックを実行する（内部メソッド）
        /// カテゴリごとの支出合計が予算上限を超えていないか確認する
        /// </summary>
        private async Task CheckBudgetAsync(int year, int month)
        {
            // 今月の支出を取得
            var transactions = await _db.Transactions
                .Where(t => t.Date.Year == year && t.Date.Month == month && t.Type == TransactionType.Expense)
                .ToListAsync();

            // カテゴリごとに予算チェック
            var categories = await _db.Categories.Where(c => c.BudgetLimit > 0).ToListAsync();
            foreach (var cat in categories)
            {
                var total = transactions.Where(t => t.CategoryId == cat.Id).Sum(t => t.Amount);
                if (total >= cat.BudgetLimit)
                {
                    // 予算超過の場合はログに出力（通知はフロント側で表示）
                    Console.WriteLine($"予算超過: {cat.Name} {total}/{cat.BudgetLimit}");
                }
            }
        }
    }

    // ============================================================
    //  リクエストモデル
    // ============================================================

    /// <summary>取引追加・更新リクエスト</summary>
    public class AddTransactionRequest
    {
        /// <summary>金額（円）</summary>
        public int Amount { get; set; }

        /// <summary>日付（yyyy-MM-dd）</summary>
        public string Date { get; set; } = "";

        /// <summary>メモ（任意）</summary>
        public string? Note { get; set; }

        /// <summary>カテゴリID</summary>
        public string CategoryId { get; set; } = "";

        /// <summary>支払い方法（Cash/Card/ElectronicMoney/QRPayment/BankAccount）</summary>
        public string PaymentMethod { get; set; } = "Cash";

        /// <summary>収支タイプ（Income/Expense）</summary>
        public string Type { get; set; } = "Expense";

        /// <summary>固定費フラグ</summary>
        public bool IsFixed { get; set; }

        /// <summary>カードID（カード払い時のみ）</summary>
        public string? CardId { get; set; }

        /// <summary>引き落とし日（yyyy-MM-dd、カード払い時のみ）</summary>
        public string? BillingDate { get; set; }
    }

    /// <summary>カテゴリ追加・更新リクエスト</summary>
    public class AddCategoryRequest
    {
        /// <summary>カテゴリ名</summary>
        public string Name { get; set; } = "";

        /// <summary>アイコン（絵文字）</summary>
        public string Icon { get; set; } = "📦";

        /// <summary>予算上限（円）0は設定なし</summary>
        public int BudgetLimit { get; set; }
    }

    /// <summary>カード追加・更新リクエスト</summary>
    public class AddCardRequest
    {
        /// <summary>カード名</summary>
        public string Name { get; set; } = "";

        /// <summary>締め日（1〜31）</summary>
        public int ClosingDay { get; set; }

        /// <summary>引き落とし日（1〜31）</summary>
        public int BillingDay { get; set; }
    }

    /// <summary>固定費追加・更新リクエスト</summary>
    public class AddFixedRequest
    {
        /// <summary>固定費名</summary>
        public string Name { get; set; } = "";

        /// <summary>金額（円）</summary>
        public int Amount { get; set; }

        /// <summary>毎月の計上日（1〜31）</summary>
        public int DayOfMonth { get; set; }

        /// <summary>カテゴリID</summary>
        public string CategoryId { get; set; } = "";

        /// <summary>支払い方法</summary>
        public string PaymentMethod { get; set; } = "Cash";
    }

    /// <summary>年月リクエスト（固定費自動計上用）</summary>
    public class YearMonthRequest
    {
        /// <summary>年</summary>
        public int Year { get; set; }

        /// <summary>月</summary>
        public int Month { get; set; }
    }
}