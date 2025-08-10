namespace platform_backend.DTOs
{
    /// <summary>
    /// استجابة تسجيل الدخول مع JWT و Refresh Token
    /// </summary>
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; } // في ثواني
        public UserInfo User { get; set; } = new();
    }

    /// <summary>
    /// معلومات المستخدم في الاستجابة
    /// </summary>
    public class UserInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public bool EmailVerified { get; set; }
    }

    /// <summary>
    /// طلب تجديد التوكن
    /// </summary>
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// طلب تسجيل الخروج
    /// </summary>
    public class LogoutRequest
    {
        public string? RefreshToken { get; set; }
    }
}