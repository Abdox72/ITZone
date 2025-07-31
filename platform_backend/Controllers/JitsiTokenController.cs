using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace platform_backend.Controllers
{
    [ApiController]
    [Route("api/jitsi")]
    public class JitsiTokenController : ControllerBase
    {
        private readonly IConfiguration _config;

        public JitsiTokenController(IConfiguration config)
            => _config = config;

        [HttpGet("token")]
        public IActionResult GetToken([FromQuery] string room, [FromQuery] string user)
        {
            // read from appsettings or .env
            var appId  = _config["JitsiService:ApplicationId"]   ?? "MyJitsiApp";
            var secret = _config["JitsiService:Secret"]  ?? "VerySecretKey";
            var domain = _config["JitsiService:Domain"]  ?? "http://localhost:8000";

            var now = DateTime.UtcNow;
            var claims = new[]
            {
                new Claim("aud", "jitsi"),
                new Claim("iss", appId),
                new Claim("sub", domain),
                new Claim("room", room),
                // embed user info:
                new Claim("context", 
                    $"{{\"user\":{{\"id\":\"{user}\",\"name\":\"{user}\"}}}}"
                )
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: appId,
                audience: "jitsi",
                claims: claims,
                notBefore: now,
                expires: now.AddHours(2),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { token = jwt });
        }
    }
}
