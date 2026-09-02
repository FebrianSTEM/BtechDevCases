using AuthWallet.Domain.Dto;

namespace AuthWallet.Application.Dtos.Wallet.Response
{
    public class WalletResponse
    {
        public decimal Balance { get; set; }
        public List<TransactioItemDto> RecentTransactions { get; set; } = new();
    }
}