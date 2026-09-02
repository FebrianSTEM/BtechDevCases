using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthWallet.Api.Controllers
{
    [Route("api/")]
    [ApiController]
    public class MeController : ControllerBase
    {
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var email = User.FindFirst(ClaimTypes.Email)
                ?? User.FindFirst(JwtRegisteredClaimNames.Email);

            if (email == null)
                return Unauthorized();

            return Ok(new { message = $"Hello {email.Value}, welcome back" });
        }
    }
}
