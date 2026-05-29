using Microsoft.EntityFrameworkCore;

namespace KakeiboApp.Models
{
    /// <summary>
    /// 家計簿アプリのデータベースコンテキスト
    /// Entity Framework Core を使って SQLite と接続する
    /// このクラスを通じてDBの読み書きを行う
    /// </summary>
    public class KakeiboDbContext : DbContext
    {
        /// <summary>
        /// コンストラクタ
        /// DIコンテナからオプションを受け取る
        /// </summary>
        public KakeiboDbContext(DbContextOptions<KakeiboDbContext> options)
            : base(options) { }

        // =====================================================
        // DBテーブルの定義
        // 各プロパティが1つのテーブルに対応する
        // =====================================================

        /// <summary>取引テーブル（収入・支出の記録）</summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>カテゴリテーブル</summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>クレジットカードテーブル</summary>
        public DbSet<CreditCard> Cards { get; set; }

        /// <summary>固定費テーブル</summary>
        public DbSet<FixedExpense> FixedExpenses { get; set; }

        /// <summary>
        /// モデルの詳細設定
        /// テーブル名・カラムの制約・初期データなどを設定する
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // =====================================================
            // カテゴリの初期データ（デフォルトカテゴリ）
            // アプリ初回起動時にDBに挿入される
            // =====================================================
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = "cat-01", Name = "食費", Icon = "🍚", BudgetLimit = 50000, IsDefault = true },
                new Category { Id = "cat-02", Name = "交通費", Icon = "🚃", BudgetLimit = 10000, IsDefault = true },
                new Category { Id = "cat-03", Name = "日用品", Icon = "🧴", BudgetLimit = 15000, IsDefault = true },
                new Category { Id = "cat-04", Name = "光熱費", Icon = "💡", BudgetLimit = 20000, IsDefault = true },
                new Category { Id = "cat-05", Name = "娯楽", Icon = "🎮", BudgetLimit = 20000, IsDefault = true },
                new Category { Id = "cat-06", Name = "医療費", Icon = "🏥", BudgetLimit = 10000, IsDefault = true },
                new Category { Id = "cat-07", Name = "衣服", Icon = "👕", BudgetLimit = 15000, IsDefault = true },
                new Category { Id = "cat-08", Name = "通信費", Icon = "📱", BudgetLimit = 10000, IsDefault = true },
                new Category { Id = "cat-09", Name = "給料", Icon = "💴", BudgetLimit = 0, IsDefault = true },
                new Category { Id = "cat-10", Name = "その他", Icon = "📦", BudgetLimit = 10000, IsDefault = true }
            );
        }
    }
}