using AuthWallet.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AuthWallet.Application.Interfaces;
using AuthWallet.Infrastructure.Authentication;
using AuthWallet.Domain.Interfaces;
using AuthWallet.Infrastructure.Persistence.Repositories;
using AuthWallet.Domain.Entities.Auth;
using AuthWallet.Domain.Entities.Wallets;
using AuthWallet.Infrastructure.Security;

namespace AuthWallet.Infrastructure
{
    public static class DepedencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //Registering Sql Server
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            // Jwt Service
            services.AddScoped<IJwtService, JwtService>();

            // Hahser
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            // Unit Of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokensRepository, RefreshTokenRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            services.AddScoped<ISessionValidator, SessionValidator>();

            return services;
        }
    }
}