namespace AuthWallet.Domain.Entities.Auth
{
    public class RefreshToken : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime LastActivityAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }


        public User User { get; set; } = null!;
    }
}