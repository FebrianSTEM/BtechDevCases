namespace AuthWallet.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IWalletRepository Wallets { get; }



        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();

        Task CommitAsync();

        Task RollbackAsync();
    }
}
