using AuthWallet.Application.Interfaces;
using AuthWallet.Application.Services;
using AuthWallet.Infrastructure;
using AuthWallet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthWallet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Infrastructure (DbContext and Repo)
            builder.Services.AddInfrastructure(builder.Configuration);

            // Add services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IWalletService, WalletService>();

            // Get JWT Configs
            var jwtSecret = builder.Configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret is not configured");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer is not configured");
            var jwtAudience = builder.Configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience is not configured");

            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     ValidIssuer = jwtIssuer,
                     ValidAudience = jwtAudience,
                     IssuerSigningKey =
                         new SymmetricSecurityKey(
                             Encoding.UTF8.GetBytes(jwtSecret)),
                     ClockSkew = TimeSpan.Zero
                 };

                 options.Events = new JwtBearerEvents
                 {
                     OnTokenValidated = async context =>
                     {
                         var validator = context.HttpContext
                             .RequestServices
                             .GetRequiredService<ISessionValidator>();

                         var userIdClaim =
                             context.Principal?
                                 .FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? context.Principal?
                                 .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                         if (!Guid.TryParse(userIdClaim, out var userId))
                         {
                             context.Fail("Invalid user token payload.");
                             return;
                         }

                         var isActive = await validator.HasActiveSessionAsync(userId);

                         if (!isActive)
                         {
                             context.Fail("Session has been revoked or expired due to inactivity.");
                         }
                     }
                 };
             });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Auto-apply migrations
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
