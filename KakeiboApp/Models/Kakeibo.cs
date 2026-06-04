using System.ComponentModel.DataAnnotations;

namespace KakeiboApp.Models
{
    /// <summary>
    /// 支払い方法
    /// </summary>
    public enum PaymentMethod
    {
        Cash,            // 現金
        Card,            // クレジットカード
        ElectronicMoney, // 電子マネー
        QRPayment,       // QR決済
        BankAccount      // 口座振替
    }

    /// <summary>
    /// 収支タイプ
    /// </summary>
    public enum TransactionType
    {
        Income,  // 収入
        Expense  // 支出
    }

    /// <summary>
    /// ユーザーアカウント
    /// メールアドレスとハッシュ化されたパスワードで認証する
    /// </summary>
    public class AppUser
    {
        /// <summary>主キー（GUID）</summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>メールアドレス（ログインID・一意）</summary>
        public string Email { get; set; } = "";

        /// <summary>パスワード（BCryptでハッシュ化して保存）</summary>
        public string PasswordHash { get; set; } = "";

        /// <summary>表示名</summary>
        public string DisplayName { get; set; } = "";

        /// <summary>登録日時</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 取引記録（収入・支出1件分のデータ）
    /// </summary>
    public class Transaction
    {
        /// <summary>主キー（GUID）</summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>所有ユーザーID</summary>
        public string UserId { get; set; } = "";

        /// <summary>使った日付</summary>
        public DateTime Date { get; set; } = DateTime.Today;

        /// <summary>引き落とし日（カード払いのみ）</summary>
        public DateTime? BillingDate { get; set; }

        /// <summary>金額（円）</summary>
        public int Amount { get; set; }

        /// <summary>カテゴリID</summary>
        public string CategoryId { get; set; } = "";

        /// <summary>メモ（任意）</summary>
        public string Note { get; set; } = "";

        /// <summary>支払い方法</summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>収支タイプ（収入 or 支出）</summary>
        public TransactionType Type { get; set; }

        /// <summary>固定費フラグ（true=毎月自動計上）</summary>
        public bool IsFixed { get; set; } = false;

        /// <summary>カードID（カード払い時のみ）</summary>
        public string? CardId { get; set; }
    }

    /// <summary>
    /// カテゴリ（食費・交通費など）
    /// </summary>
    public class Category
    {
        /// <summary>主キー</summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>所有ユーザーID</summary>
        public string UserId { get; set; } = "";

        /// <summary>カテゴリ名</summary>
        public string Name { get; set; } = "";

        /// <summary>予算上限（円）0は設定なし</summary>
        public int BudgetLimit { get; set; } = 0;

        /// <summary>デフォルトカテゴリフラグ（削除不可）</summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>アイコン（絵文字）</summary>
        public string Icon { get; set; } = "💰";
    }

    /// <summary>
    /// クレジットカード情報
    /// </summary>
    public class CreditCard
    {
        /// <summary>主キー</summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>所有ユーザーID</summary>
        public string UserId { get; set; } = "";

        /// <summary>カード名（例：楽天カード）</summary>
        public string Name { get; set; } = "";

        /// <summary>締め日（1〜31）</summary>
        public int ClosingDay { get; set; }

        /// <summary>引き落とし日（1〜31）</summary>
        public int BillingDay { get; set; }
    }

    /// <summary>
    /// 固定費テンプレート
    /// 毎月自動で計上される支出（家賃・サブスクなど）
    /// </summary>
    public class FixedExpense
    {
        /// <summary>主キー</summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>所有ユーザーID</summary>
        public string UserId { get; set; } = "";

        /// <summary>固定費名（例：家賃・Netflix）</summary>
        public string Name { get; set; } = "";

        /// <summary>金額（円）</summary>
        public int Amount { get; set; }

        /// <summary>カテゴリID</summary>
        public string CategoryId { get; set; } = "";

        /// <summary>支払い方法</summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>毎月何日に計上するか（1〜31）</summary>
        public int DayOfMonth { get; set; }
    }
}