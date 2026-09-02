using AuthWallet.Application.Dtos.Wallet.Request;
using AuthWallet.Application.Dtos.Wallet.Response;

namespace AuthWallet.Application.Interfaces
{
    public interface IWalletService
    {
        Task<WalletResponse> GetWallet(Guid userId);
        Task<TransferResponse> Transfer(Guid userId, TransferRequest request);

    }
}
