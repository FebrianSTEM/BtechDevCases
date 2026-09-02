using AuthWallet.Domain.Entities;
using AuthWallet.Domain.Entities.Auth;
using AuthWallet.Domain.Entities.Wallets;
using Microsoft.EntityFrameworkCore;

namespace AuthWallet.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

        public DbSet<User> Users => Set<User>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<Wallet> Wallets => Set<Wallet>();

        public DbSet<Transaction> Transactions => Set<Transaction>(); 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(128);
                entity.Property(e => e.PasswordHash).IsRequired();
            });


            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TokenHash);
                entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(256);
                
                entity.HasOne(e => e.User)
                .WithMany(e => e.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Balance).HasPrecision(18, 2);

                entity.HasOne(e => e.User)
                .WithOne(e => e.Wallet)
                .HasForeignKey<Wallet>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.HasIndex(e => e.IdempotencyKey).IsUnique();
                entity.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);

                // Sender Relationship
                entity.HasOne(e => e.SenderWallet)
                .WithMany(e => e.SentTransactions)
                .HasForeignKey(e => e.SenderWalletId)
                .OnDelete(DeleteBehavior.Restrict);

                // Recepient Relationship
                entity.HasOne(e => e.RecipientWallet)
                .WithMany(e => e.ReceivedTransactions)
                .HasForeignKey(e => e.RecipientWalletId)
                .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.Now;
                    entry.Entity.UpdatedAt = entry.Entity.CreatedAt;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.Now;
                }
            }
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.Now;
                    entry.Entity.UpdatedAt = entry.Entity.CreatedAt;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.Now;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
