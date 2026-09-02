using System.ComponentModel.DataAnnotations;

namespace AuthWallet.Application.Dtos.Wallet.Request
{
    public class TransferRequest
    {
        [Required(ErrorMessage = "Recipient email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string RecipientEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is Required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public string? Notes { get; set; }

        [Required(ErrorMessage = "Idempotency key is required")]
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}