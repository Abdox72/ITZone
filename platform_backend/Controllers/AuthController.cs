using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using platform_backend.DTOs;
using platform_backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace platform_backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                // هنا ممكن تتحقق من وجوده في قاعدة البيانات أو تسجله

                var token = GenerateJwtToken(payload.Email, payload.Name);

                var response = new AuthResponse
                {
                    Token = token,
                    Email = payload.Email,
                    Name = payload.Name
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = "فشل التحقق من Google Token", detail = ex.Message });
            }
        }

        private string GenerateJwtToken(string email, string name)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpOptions("google-login")]
        public IActionResult GoogleLoginOptions()
        {
            return Ok();
        }

    }
}
