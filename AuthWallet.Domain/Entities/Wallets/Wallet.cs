using AuthWallet.Domain.Entities.Auth;

namespace AuthWallet.Domain.Entities.Wallets
{
    public class Wallet : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }

        // Nav
        public User User { get; set; } = null!;
        public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
        public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
    }
}