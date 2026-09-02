namespace AuthWallet.Application.Interfaces
{
    public interface ISessionValidator
    {
        Task<bool> HasActiveSessionAsync(Guid userId);
    }
}