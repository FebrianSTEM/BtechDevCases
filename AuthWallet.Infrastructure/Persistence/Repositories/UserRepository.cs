using AuthWallet.Domain.Entities.Auth;
using AuthWallet.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AuthWallet.Infrastructure.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) 
        {
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.AsNoTracking()
                                       .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetUserWalletByEmail(string email)
        {
            return await _context.Users.AsNoTracking()
                .Include(x => x.Wallet)
                .FirstOrDefaultAsync( x=> x.Email == email);
        }
    }
}
