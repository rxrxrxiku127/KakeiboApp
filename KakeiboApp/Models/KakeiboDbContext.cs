using Microsoft.EntityFrameworkCore;

namespace KakeiboApp.Models
{
    /// <summary>
    /// 家計簿アプリのデータベースコンテキスト
    /// SQLiteと接続してデータの読み書きを行う
    /// </summary>
    public class KakeiboDbContext : DbContext
    {
        public KakeiboDbContext(DbContextOptions<KakeiboDbContext> options)
            : base(options) { }

        // =====================================================
        // DBテーブルの定義
        // =====================================================

        /// <summary>ユーザーテーブル</summary>
        public DbSet<AppUser> Users { get; set; }

        /// <summary>取引テーブル</summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>カテゴリテーブル</summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>クレジットカードテーブル</summary>
        public DbSet<CreditCard> Cards { get; set; }

        /// <summary>固定費テーブル</summary>
        public DbSet<FixedExpense> FixedExpenses { get; set; }

        /// <summary>
        /// モデルの詳細設定
        /// インデックス・制約などを設定する
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // メールアドレスはユニーク制約
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}