using AuthWallet.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace AuthWallet.Domain.Dto
{
    public class TransferRequest
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
        public User RecipientUser { get; set; } = new();
        public Guid SenderWalletId { get; set; }

    }
}
