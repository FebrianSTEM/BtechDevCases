using AuthWallet.Application.Dtos.Auth.Request;
using AuthWallet.Application.Dtos.Auth.Response;
using AuthWallet.Application.Interfaces;
using AuthWallet.Domain.Entities.Auth;
using AuthWallet.Domain.Entities.Wallets;
using AuthWallet.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AuthWallet.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokensRepository _refreshTokenRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IRefreshTokensRepository refreshTokenRepository,
            IWalletRepository walletRepository,
            IJwtService jwtService,
            IConfiguration configuration,
            IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _walletRepository = walletRepository;
            _jwtService = jwtService;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetUserByEmail(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email is already registered");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.ToLowerInvariant(),
                PasswordHash = _passwordHasher.Hash(request.Password)
            };

            var defaultBalance = decimal.Parse(_configuration["DefaultBalance"]
                ?? throw new InvalidOperationException("DefaultBalance is not configured"));

            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Balance = defaultBalance
            };

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenEntity = GenerateRefreshTokenEntity(user.Id, refreshToken);

            using var transaction = _unitOfWork.BeginTransactionAsync();
            try
            {
                await _userRepository.AddAsync(user);
                await _refreshTokenRepository.AddAsync(refreshTokenEntity);
                await _walletRepository.AddAsync(wallet);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiryAt = GetExpiryToken()
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetUserByEmail(request.Email.ToLowerInvariant());
            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password");

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenEntity = GenerateRefreshTokenEntity(user.Id, refreshToken);

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiryAt = GetExpiryToken()
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var tokenHash = _jwtService.HashToken(request.RefreshToken);
            var inactivityMinutes = int.Parse(_configuration["Jwt:InactivityTimeoutMinutes"]
            ?? throw new InvalidOperationException("Jwt:InactivityTimeoutMinutes is not configured in appsettings.json"));

            var storedToken = await _refreshTokenRepository.GetStoredToken(tokenHash);

            if (storedToken == null)
                throw new UnauthorizedAccessException("Invalid refresh token");

            if(storedToken.IsRevoked)
                throw new UnauthorizedAccessException("Refresh token has been revoked");

            if (storedToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired");
            
            if((DateTime.UtcNow - storedToken.LastActivityAt).TotalMinutes > inactivityMinutes)
            {
                storedToken.IsRevoked = true;
                await _refreshTokenRepository.UpdateAsync(storedToken);
                await _unitOfWork.SaveChangesAsync();
                throw new UnauthorizedAccessException("Session expired");
            }

            storedToken.LastActivityAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            var accessToken = _jwtService.GenerateAccessToken(storedToken.User);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = request.RefreshToken,
                ExpiryAt = GetExpiryToken()
            };
        }

        public async Task LogoutAsync(Guid userId, string? refreshToken)
        {
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var tokenHash = _jwtService.HashToken(refreshToken);
                var storedToken = await _refreshTokenRepository.GetStoredToken(tokenHash);

                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    await _refreshTokenRepository.UpdateAsync(storedToken);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            await RevokedUserActiveTokens(userId);
        }

        public async Task RevokedUserActiveTokens(Guid userId)
        {
            List<RefreshToken> activeTokens = await _refreshTokenRepository.GetUserActiveRefreshToken(userId);
            foreach (RefreshToken token in activeTokens)
            {
                token.IsRevoked = true;
            }
            if (activeTokens.Any())
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public DateTime GetExpiryToken()
        {
            var expiryMinutes = int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"]
            ?? throw new InvalidOperationException("Jwt:AccessTokenExpiryMinutes is not configured in appsettings.json"));

            return DateTime.UtcNow.AddMinutes(expiryMinutes);
        }
      

        public RefreshToken GenerateRefreshTokenEntity(Guid userId, string refreshToken)
        {
            var refreshTokenExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]
                ?? throw new InvalidOperationException("Jwt:RefreshTokenExpiryDays is not configured"));

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = _jwtService.HashToken(refreshToken),
                LastActivityAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            return refreshTokenEntity;
        }
    }
}
