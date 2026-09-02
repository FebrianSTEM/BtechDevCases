using AuthWallet.Domain.Dto;
using AuthWallet.Domain.Entities.Wallets;
using AuthWallet.Domain.Enums;
using AuthWallet.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthWallet.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<TransactioItemDto>> GetLatestTransaction(Guid userId, Guid walletId, int topData = 10)
        {
            var sentTransactions = await _context.Transactions
                .Where(x => x.SenderWalletId == walletId)
                .Include(x => x.RecipientWallet)
                    .ThenInclude(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .Take(topData)
                .Select(x => new TransactioItemDto
                {
                    Id = x.Id,
                    Direction = "sent",
                    CounterPartyEmail = x.RecipientWallet.User.Email,
                    Amount = x.Amount,
                    Notes = x.Notes,
                    Status = x.Status.ToString(),
                    CreatedAt = x.CreatedAt
                }).ToListAsync();

            var receivedTransactions = await _context.Transactions
               .Where(x => x.RecipientWalletId == walletId)
               .Include(x => x.SenderWallet)
                   .ThenInclude(x => x.User)
               .OrderByDescending(x => x.CreatedAt)
               .Take(topData)
               .Select(x => new TransactioItemDto
               {
                   Id = x.Id,
                   Direction = "received",
                   CounterPartyEmail = x.SenderWallet.User.Email,
                   Amount = x.Amount,
                   Notes = x.Notes,
                   Status = x.Status.ToString(),
                   CreatedAt = x.CreatedAt
               }).ToListAsync();

            return sentTransactions
                .Concat(receivedTransactions)
                .OrderByDescending(x => x.CreatedAt)
                .Take(topData)
                .ToList();
        }

        public async Task<Transaction?> TransactionByIdempotency(string idempotencyKey)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey);
        }

        public async Task<Transaction> Transfer(TransferRequest request)
        {
            var freshSenderWallet = await _context.Wallets
                .FirstAsync(w => w.Id == request.SenderWalletId);

            if (freshSenderWallet.Balance < request.Amount)
            {
                throw new ArgumentException("Insufficient balance");
            }

            // Debit sender
            freshSenderWallet.Balance -= request.Amount;
            freshSenderWallet.UpdatedAt = DateTime.UtcNow;

            // Credit recipient
            var recipientWallet = await _context.Wallets
                .FirstAsync(w => w.Id == request.RecipientUser.Wallet.Id);
            recipientWallet.Balance += request.Amount;
            recipientWallet.UpdatedAt = DateTime.UtcNow;

            // Create transaction record
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                SenderWalletId = freshSenderWallet.Id,
                RecipientWalletId = recipientWallet.Id,
                Amount = request.Amount,
                Notes = request.Notes,
                Status = TransactionStatus.Completed,
                IdempotencyKey = request.IdempotencyKey,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            return transaction;
        }
    }
}