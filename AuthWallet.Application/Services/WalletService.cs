using AuthWallet.Application.Dtos.Wallet.Request;
using AuthWallet.Application.Dtos.Wallet.Response;
using AuthWallet.Application.Interfaces;
using AuthWallet.Domain.Dto;
using AuthWallet.Domain.Entities.Auth;
using AuthWallet.Domain.Entities.Wallets;
using AuthWallet.Domain.Interfaces;
using TransferRequest = AuthWallet.Application.Dtos.Wallet.Request.TransferRequest;

namespace AuthWallet.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;

        public WalletService(IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            ITransactionRepository transactionRepository)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<WalletResponse> GetWallet(Guid userId)
        {
            Wallet wallet = await GetAndValidateWallet(userId);

            List<TransactioItemDto> transactionItems = await _transactionRepository.GetLatestTransaction(userId, wallet.Id);

            return new WalletResponse()
            {
                Balance = wallet.Balance,
                RecentTransactions = transactionItems
            };
        }

        public async Task<TransferResponse> Transfer(Guid userId, TransferRequest request)
        {
            var existingTransaction = await _transactionRepository
                .TransactionByIdempotency(request.IdempotencyKey);
            if (existingTransaction != null)
            {
                var recipientUser = await _walletRepository.GetWalletUserByWalletId(existingTransaction.RecipientWalletId);

                return new TransferResponse()
                {
                    TransactionId = existingTransaction.Id,
                    RecipientEmail = recipientUser?.User.Email ?? "",
                    Amount = existingTransaction.Amount,
                    Notes = existingTransaction.Notes,
                    Status = existingTransaction.Status.ToString(),
                    CreatedAt = existingTransaction.CreatedAt
                };
            }

            Wallet senderWallet = await GetAndValidateWallet(userId);

            User recipient = await GetAndValidateRecipient(request.RecipientEmail);

            ValidateSelfSending(recipient.Id, userId);

            ValidateBalance(senderWallet.Balance, request.Amount);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                Transaction transaction = 
                await _transactionRepository.Transfer(new Domain.Dto.TransferRequest()
                {
                   RecipientEmail = request.RecipientEmail,
                   Amount = request.Amount,
                   Notes = request.Notes,
                   IdempotencyKey = request.IdempotencyKey,
                   RecipientUser = recipient,
                   SenderWalletId = senderWallet.Id
                });
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new TransferResponse()
                {
                    TransactionId = transaction.Id,
                    RecipientEmail = recipient.Email,
                    Amount = transaction.Amount,
                    Notes = transaction.Notes,
                    Status = transaction.Status.ToString(),
                    CreatedAt = transaction.CreatedAt
                };
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

        }

        private async Task<Wallet> GetAndValidateWallet(Guid userId)
        {
            return await _walletRepository.GetWalletByUser(userId)
             ?? throw new InvalidOperationException("Wallet not found");
        }

        private async Task<User> GetAndValidateRecipient(string emailRecipient)
        {
            return await _userRepository.GetUserWalletByEmail(emailRecipient.ToLowerInvariant())
                ?? throw new InvalidOperationException("Recipient not found");
        }

        private void ValidateSelfSending(Guid recipientId, Guid userId)
        {
            if (recipientId == userId)
                throw new InvalidOperationException("Cannot transfer to yourself");
        }

        private void ValidateBalance(decimal balance, decimal nominalTransfer)
        {
            if (balance < nominalTransfer)
                throw new InvalidOperationException("Insufficient balance");
        }
    }
}
