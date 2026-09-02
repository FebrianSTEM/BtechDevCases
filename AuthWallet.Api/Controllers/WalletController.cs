using AuthWallet.Application.Dtos.Wallet.Request;
using AuthWallet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthWallet.Api.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {

        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }


        [HttpGet("wallet")]
        public async Task<IActionResult> GetWallet()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _walletService.GetWallet(userId.Value);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("wallet/transfer")]
        public async Task<IActionResult> TransferFund([FromBody] TransferRequest request)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();
            try
            {
                var result = await _walletService.Transfer(userId.Value, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                return userId;
            return null;
        }

    }
}
