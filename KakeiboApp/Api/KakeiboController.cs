using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using KakeiboApp.Models;

namespace KakeiboApp.Api
{
    /// <summary>
    /// 家計簿APIコントローラー
    /// 認証済みユーザーのみアクセス可能
    /// </summary>
    [ApiController]
    [Route("api/kakeibo")]
    [Authorize] // ログイン必須
    public class KakeiboController : ControllerBase
    {
        private readonly KakeiboDbContext _db;

        public KakeiboController(KakeiboDbContext db)
        {
            _db = db;
        }

        // =====================================================
        // ログイン中のユーザーIDを取得するヘルパー
        // =====================================================
        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        // ============================================================
        // 取引（Transaction）
        // ============================================================

        /// <summary>取引を追加する POST /api/kakeibo/transaction</summary>
        [HttpPost("transaction")]
        public async Task<IActionResult> AddTransaction([FromBody] AddTransactionRequest req)
        {
            var transaction = new Transaction
            {
                UserId = GetUserId(),
                Amount = req.Amount,
                Date = DateTime.Parse(req.Date),
                Note = req.Note ?? "",
                CategoryId = req.CategoryId,
                PaymentMethod = Enum.Parse<PaymentMethod>(req.PaymentMethod),
                Type = Enum.Parse<TransactionType>(req.Type),
                IsFixed = req.IsFixed,
                CardId = req.CardId,
                BillingDate = string.IsNullOrEmpty(req.BillingDate)
                                ? null : DateTime.Parse(req.BillingDate)
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            return Ok(new { message = "登録しました！", id = transaction.Id });
        }

        /// <summary>取引を更新する PUT /api/kakeibo/transaction/{id}</summary>
        [HttpPut("transaction/{id}")]
        public async Task<IActionResult> UpdateTransaction(string id, [FromBody] AddTransactionRequest req)
        {
            var item = await _db.Transactions.FirstOrDefaultAsync(
                t => t.Id == id && t.UserId == GetUserId());
            if (item == null) return NotFound();

            item.Amount = req.Amount;
            item.Date = DateTime.Parse(req.Date);
            item.Note = req.Note ?? "";
            item.CategoryId = req.CategoryId;
            item.PaymentMethod = Enum.Parse<PaymentMethod>(req.PaymentMethod);
            item.Type = Enum.Parse<TransactionType>(req.Type);
            item.IsFixed = req.IsFixed;
            item.CardId = req.CardId;
            item.BillingDate = string.IsNullOrEmpty(req.BillingDate)
                                 ? null : DateTime.Parse(req.BillingDate);

            await _db.SaveChangesAsync();
            return Ok(new { message = "更新しました！" });
        }

        /// <summary>取引を削除する DELETE /api/kakeibo/transaction/{id}</summary>
        [HttpDelete("transaction/{id}")]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            var item = await _db.Transactions.FirstOrDefaultAsync(
                t => t.Id == id && t.UserId == GetUserId());
            if (item == null) return NotFound();

            _db.Transactions.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "削除しました！" });
        }

        // ============================================================
        // カテゴリ（Category）
        // ============================================================

        /// <summary>カテゴリを追加する POST /api/kakeibo/category</summary>
        [HttpPost("category")]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest req)
        {
            var category = new Category
            {
                UserId = GetUserId(),
                Name = req.Name,
                Icon = req.Icon,
                BudgetLimit = req.BudgetLimit,
                IsDefault = false
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return Ok(new { message = "登録しました！", id = category.Id });
        }

        /// <summary>カテゴリを更新する PUT /api/kakeibo/category/{id}</summary>
        [HttpPut("category/{id}")]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] AddCategoryRequest req)
        {
            var item = await _db.Categories.FirstOrDefaultAsync(
                c => c.Id == id && c.UserId == GetUserId());
            if (item == null) return NotFound();

            item.Name = req.Name;
            item.Icon = req.Icon;
            item.BudgetLimit = req.BudgetLimit;

            await _db.SaveChangesAsync();
            return Ok(new { message = "更新しました！" });
        }

        /// <summary>カテゴリを削除する DELETE /api/kakeibo/category/{id}</summary>
        [HttpDelete("category/{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var item = await _db.Categories.FirstOrDefaultAsync(
                c => c.Id == id && c.UserId == GetUserId());
            if (item == null) return NotFound();

            if (item.IsDefault)
                return BadRequest(new { message = "デフォルトカテゴリは削除できません" });

            _db.Categories.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "削除しました！" });
        }

        // ============================================================
        // カード（CreditCard）
        // ============================================================

        /// <summary>カードを追加する POST /api/kakeibo/card</summary>
        [HttpPost("card")]
        public async Task<IActionResult> AddCard([FromBody] AddCardRequest req)
        {
            var card = new CreditCard
            {
                UserId = GetUserId(),
                Name = req.Name,
                ClosingDay = req.ClosingDay,
                BillingDay = req.BillingDay
            };

            _db.Cards.Add(card);
            await _db.SaveChangesAsync();
            return Ok(new { message = "登録しました！", id = card.Id });
        }

        /// <summary>カードを更新する PUT /api/kakeibo/card/{id}</summary>
        [HttpPut("card/{id}")]
        public async Task<IActionResult> UpdateCard(string id, [FromBody] AddCardRequest req)
        {
            var item = await _db.Cards.FirstOrDefaultAsync(
                c => c.Id == id && c.UserId == GetUserId());
            if (item == null) return NotFound();

            item.Name = req.Name;
            item.ClosingDay = req.ClosingDay;
            item.BillingDay = req.BillingDay;

            await _db.SaveChangesAsync();
            return Ok(new { message = "更新しました！" });
        }

        /// <summary>カードを削除する DELETE /api/kakeibo/card/{id}</summary>
        [HttpDelete("card/{id}")]
        public async Task<IActionResult> DeleteCard(string id)
        {
            var item = await _db.Cards.FirstOrDefaultAsync(
                c => c.Id == id && c.UserId == GetUserId());
            if (item == null) return NotFound();

            _db.Cards.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "削除しました！" });
        }

        // ============================================================
        // 固定費（FixedExpense）
        // ============================================================

        /// <summary>固定費を追加する POST /api/kakeibo/fixed</summary>
        [HttpPost("fixed")]
        public async Task<IActionResult> AddFixed([FromBody] AddFixedRequest req)
        {
            var fixed_ = new FixedExpense
            {
                UserId = GetUserId(),
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

        /// <summary>固定費を更新する PUT /api/kakeibo/fixed/{id}</summary>
        [HttpPut("fixed/{id}")]
        public async Task<IActionResult> UpdateFixed(string id, [FromBody] AddFixedRequest req)
        {
            var item = await _db.FixedExpenses.FirstOrDefaultAsync(
                f => f.Id == id && f.UserId == GetUserId());
            if (item == null) return NotFound();

            item.Name = req.Name;
            item.Amount = req.Amount;
            item.DayOfMonth = req.DayOfMonth;
            item.CategoryId = req.CategoryId;
            item.PaymentMethod = Enum.Parse<PaymentMethod>(req.PaymentMethod);

            await _db.SaveChangesAsync();
            return Ok(new { message = "更新しました！" });
        }

        /// <summary>固定費を削除する DELETE /api/kakeibo/fixed/{id}</summary>
        [HttpDelete("fixed/{id}")]
        public async Task<IActionResult> DeleteFixed(string id)
        {
            var item = await _db.FixedExpenses.FirstOrDefaultAsync(
                f => f.Id == id && f.UserId == GetUserId());
            if (item == null) return NotFound();

            _db.FixedExpenses.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "削除しました！" });
        }

        // ============================================================
        // 固定費の自動計上
        // ============================================================

        /// <summary>固定費を今月分として自動登録する POST /api/kakeibo/apply-fixed</summary>
        [HttpPost("apply-fixed")]
        public async Task<IActionResult> ApplyFixed([FromBody] YearMonthRequest req)
        {
            var userId = GetUserId();
            var fixedList = await _db.FixedExpenses
                .Where(f => f.UserId == userId).ToListAsync();

            foreach (var f in fixedList)
            {
                var exists = await _db.Transactions.AnyAsync(t =>
                    t.UserId == userId &&
                    t.IsFixed &&
                    t.Note == f.Name &&
                    t.Date.Year == req.Year &&
                    t.Date.Month == req.Month);

                if (!exists)
                {
                    var day = Math.Min(f.DayOfMonth, DateTime.DaysInMonth(req.Year, req.Month));
                    var date = new DateTime(req.Year, req.Month, day);

                    _db.Transactions.Add(new Transaction
                    {
                        UserId = userId,
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
    }

    // ============================================================
    // リクエストモデル
    // ============================================================

    public class AddTransactionRequest
    {
        public int Amount { get; set; }
        public string Date { get; set; } = "";
        public string? Note { get; set; }
        public string CategoryId { get; set; } = "";
        public string PaymentMethod { get; set; } = "Cash";
        public string Type { get; set; } = "Expense";
        public bool IsFixed { get; set; }
        public string? CardId { get; set; }
        public string? BillingDate { get; set; }
    }

    public class AddCategoryRequest
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "📦";
        public int BudgetLimit { get; set; }
    }

    public class AddCardRequest
    {
        public string Name { get; set; } = "";
        public int ClosingDay { get; set; }
        public int BillingDay { get; set; }
    }

    public class AddFixedRequest
    {
        public string Name { get; set; } = "";
        public int Amount { get; set; }
        public int DayOfMonth { get; set; }
        public string CategoryId { get; set; } = "";
        public string PaymentMethod { get; set; } = "Cash";
    }

    public class YearMonthRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }
}