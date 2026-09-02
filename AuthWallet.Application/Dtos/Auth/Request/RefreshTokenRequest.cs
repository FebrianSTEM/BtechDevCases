using System.ComponentModel.DataAnnotations;

namespace AuthWallet.Application.Dtos.Auth.Request
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}