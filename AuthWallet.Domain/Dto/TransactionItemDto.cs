namespace AuthWallet.Domain.Dto
{
    public class TransactioItemDto
    {
        public Guid Id { get; set; }
        public string Direction { get; set; } = string.Empty;
        public string CounterPartyEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}