namespace AuthWallet.Application.Dtos.Wallet.Response
{
    public class TransferResponse
    {
        public Guid TransactionId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}