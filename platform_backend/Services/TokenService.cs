using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using platform_backend.Data;
using platform_backend.DTOs;
using platform_backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace platform_backend.Services
{
    /// <summary>
    /// خدمة إدارة التوكنات (JWT و Refresh Tokens)
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IConfiguration configuration,
            AppDbContext context,
            ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// إنشاء JWT Access Token
        /// </summary>
        public string GenerateAccessToken(User user)
        {
            var jwtKey = _configuration["Authentication:Jwt:Key"] ?? 
                        throw new InvalidOperationException("JWT Key not configured");
            var issuer = _configuration["Authentication:Jwt:Issuer"];
            var audience = _configuration["Authentication:Jwt:Audience"];
            var accessTokenMinutes = int.Parse(_configuration["Authentication:Jwt:AccessTokenMinutes"] ?? "15");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.DisplayName),
                new("google_id", user.GoogleId),
                new("email_verified", user.EmailVerified.ToString().ToLower())
            };

            if (!string.IsNullOrEmpty(user.PhotoUrl))
            {
                claims.Add(new Claim("picture", user.PhotoUrl));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// إنشاء Refresh Token
        /// </summary>
        public async Task<RefreshToken> GenerateRefreshTokenAsync(User user, string? userAgent = null, string? ipAddress = null)
        {
            var refreshTokenDays = int.Parse(_configuration["Authentication:Jwt:RefreshTokenDays"] ?? "30");
            
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = GenerateSecureRandomToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
                UserAgent = userAgent,
                IpAddress = ipAddress
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        /// <summary>
        /// التحقق من صحة JWT Token
        /// </summary>
        public bool ValidateAccessToken(string token)
        {
            try
            {
                var jwtKey = _configuration["Authentication:Jwt:Key"] ?? 
                            throw new InvalidOperationException("JWT Key not configured");
                var issuer = _configuration["Authentication:Jwt:Issuer"];
                var audience = _configuration["Authentication:Jwt:Audience"];

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid JWT token");
                return false;
            }
        }

        /// <summary>
        /// الحصول على معلومات المستخدم من JWT Token
        /// </summary>
        public int? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                
                var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract user ID from token");
            }

            return null;
        }

        /// <summary>
        /// تجديد Access Token باستخدام Refresh Token
        /// </summary>
        public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken, string? userAgent = null, string? ipAddress = null)
        {
            try
            {
                var storedToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (storedToken == null || !storedToken.IsValid)
                {
                    _logger.LogWarning("Invalid or expired refresh token");
                    return null;
                }

                var user = storedToken.User;
                if (!user.IsActive)
                {
                    _logger.LogWarning("User account is inactive");
                    return null;
                }

                // Update last login
                user.LastLogin = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                // Generate new tokens
                var newAccessToken = GenerateAccessToken(user);
                var newRefreshToken = await GenerateRefreshTokenAsync(user, userAgent, ipAddress);

                // Optionally revoke the old refresh token (token rotation)
                storedToken.Revoke();

                await _context.SaveChangesAsync();

                var accessTokenMinutes = int.Parse(_configuration["Authentication:Jwt:AccessTokenMinutes"] ?? "15");

                return new AuthResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    ExpiresIn = accessTokenMinutes * 60, // Convert to seconds
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        PhotoUrl = user.PhotoUrl,
                        EmailVerified = user.EmailVerified
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return null;
            }
        }

        /// <summary>
        /// إلغاء Refresh Token
        /// </summary>
        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (storedToken == null)
                    return false;

                storedToken.Revoke();
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                return false;
            }
        }

        /// <summary>
        /// إلغاء جميع Refresh Tokens للمستخدم
        /// </summary>
        public async Task<bool> RevokeAllUserTokensAsync(int userId)
        {
            try
            {
                var userTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                    .ToListAsync();

                foreach (var token in userTokens)
                {
                    token.Revoke();
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all user tokens");
                return false;
            }
        }

        /// <summary>
        /// تنظيف التوكنات المنتهية الصلاحية
        /// </summary>
        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _context.RefreshTokens
                    .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync();

                _context.RefreshTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} expired refresh tokens", expiredTokens.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired tokens");
            }
        }

        /// <summary>
        /// إنشاء توكن عشوائي آمن
        /// </summary>
        private static string GenerateSecureRandomToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[64];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}