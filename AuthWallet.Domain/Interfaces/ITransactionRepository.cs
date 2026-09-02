using AuthWallet.Domain.Dto;
using AuthWallet.Domain.Entities.Wallets;

namespace AuthWallet.Domain.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<List<TransactioItemDto>> GetLatestTransaction(Guid userId, Guid walletId, int topData = 10);
        Task<Transaction?> TransactionByIdempotency(string idempotencyKey);
        Task<Transaction> Transfer(TransferRequest request);
    }
}