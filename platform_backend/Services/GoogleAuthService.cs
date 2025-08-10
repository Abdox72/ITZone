using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using platform_backend.Data;
using platform_backend.Models;
using System.Text.Json;

namespace platform_backend.Services
{
    /// <summary>
    /// خدمة المصادقة عبر Google OAuth2
    /// </summary>
    public class GoogleAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            AppDbContext context,
            ILogger<GoogleAuthService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// إنشاء رابط المصادقة مع Google
        /// </summary>
        public string GetGoogleAuthUrl(string state = "")
        {
            var clientId = _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
            var redirectUri = _configuration["Google:RedirectUri"] ?? throw new InvalidOperationException("Google RedirectUri not configured");
            
            var authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
                         $"?client_id={clientId}" +
                         $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                         "&response_type=code" +
                         "&scope=openid email profile" +
                         "&access_type=offline" +
                         "&prompt=consent" +
                         $"&state={state}";

            return authUrl;
        }

        /// <summary>
        /// تبديل الكود للحصول على التوكن
        /// </summary>
        public async Task<GoogleTokenResponse?> ExchangeCodeForTokenAsync(string code)
        {
            try
            {
                var clientId = _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
                var clientSecret = _configuration["Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
                var redirectUri = _configuration["Google:RedirectUri"] ?? throw new InvalidOperationException("Google RedirectUri not configured");

                var values = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("فشل في تبديل الكود: {Error}", errorContent);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تبديل الكود للتوكن");
                return null;
            }
        }

        /// <summary>
        /// التحقق من صحة ID Token والحصول على معلومات المستخدم
        /// </summary>
        public async Task<GoogleJsonWebSignature.Payload?> ValidateIdTokenAsync(string idToken)
        {
            try
            {
                var clientId = _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في التحقق من ID Token");
                return null;
            }
        }

        /// <summary>
        /// إنشاء أو تحديث المستخدم في قاعدة البيانات
        /// </summary>
        public async Task<User> CreateOrUpdateUserAsync(Google.Apis.Auth.GoogleJsonWebSignature.Payload payload)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                // إنشاء مستخدم جديد
                user = new User
                {
                    Email = payload.Email,
                    DisplayName = payload.Name,
                    PhotoUrl = payload.Picture,
                    EmailVerified = payload.EmailVerified,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // تحديث بيانات المستخدم لو اتغيرت
                user.DisplayName = payload.Name;
                user.PhotoUrl = payload.Picture;
                user.EmailVerified = payload.EmailVerified;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            return user;
        }

    }

    /// <summary>
    /// استجابة التوكن من Google
    /// </summary>
    public class GoogleTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
    }
}