using platform_backend.DTOs;
using platform_backend.Models;

namespace platform_backend.Services
{
    /// <summary>
    /// واجهة خدمة إدارة التوكنات
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// إنشاء JWT Access Token
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// إنشاء Refresh Token
        /// </summary>
        Task<RefreshToken> GenerateRefreshTokenAsync(User user, string? userAgent = null, string? ipAddress = null);

        /// <summary>
        /// التحقق من صحة JWT Token
        /// </summary>
        bool ValidateAccessToken(string token);

        /// <summary>
        /// الحصول على معلومات المستخدم من JWT Token
        /// </summary>
        int? GetUserIdFromToken(string token);

        /// <summary>
        /// تجديد Access Token باستخدام Refresh Token
        /// </summary>
        Task<AuthResponse?> RefreshTokenAsync(string refreshToken, string? userAgent = null, string? ipAddress = null);

        /// <summary>
        /// إلغاء Refresh Token
        /// </summary>
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// إلغاء جميع Refresh Tokens للمستخدم
        /// </summary>
        Task<bool> RevokeAllUserTokensAsync(int userId);

        /// <summary>
        /// تنظيف التوكنات المنتهية الصلاحية
        /// </summary>
        Task CleanupExpiredTokensAsync();
    }
}