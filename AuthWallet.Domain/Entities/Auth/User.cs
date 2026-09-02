using AuthWallet.Domain.Entities.Wallets;

namespace AuthWallet.Domain.Entities.Auth
{
    public class User : BaseEntity
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public Wallet Wallet { get; set; } = null!;
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
