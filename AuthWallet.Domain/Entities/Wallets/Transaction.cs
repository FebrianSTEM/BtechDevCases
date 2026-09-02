using AuthWallet.Domain.Enums;

namespace AuthWallet.Domain.Entities.Wallets
{
    public class Transaction : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid SenderWalletId { get; set; }
        public Guid RecipientWalletId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public TransactionStatus Status { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;

        // Nav
        public Wallet SenderWallet { get; set; } = null!;
        public Wallet RecipientWallet { get; set; } = null!;
    }
}
