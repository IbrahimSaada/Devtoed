// Devoted.API/Controllers/AuthController.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Devoted.Domain.Sql.Request;
using Devoted.Domain.Sql.Response.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Devoted.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _cfg;

        public AuthController(IConfiguration cfg) => _cfg = cfg;

        [HttpPost("token")]
        [AllowAnonymous]
        public IActionResult GetToken(LoginRequest req)
        {
            if (req.UserName != "demo" || req.Password != "Pa$$w0rd")
                return Unauthorized("Invalid credentials");

            var jwt = _cfg.GetSection("Jwt");
            var key = jwt["Key"] ?? throw new InvalidOperationException("Missing Jwt:Key");
            var iss = jwt["Issuer"] ?? throw new InvalidOperationException("Missing Jwt:Issuer");
            var aud = jwt["Audience"] ?? throw new InvalidOperationException("Missing Jwt:Audience");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: iss,
                audience: aud,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds,
                claims: new[] { new Claim("sub", req.UserName) }
            );

            return Ok(new BaseResponse
            {
                Message = "Token generated",
                Data = new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token) }
            });
        }


    }
}
