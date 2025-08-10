using System.ComponentModel.DataAnnotations;

namespace platform_backend.DTOs
{
    /// <summary>
    /// طلب تسجيل الدخول باستخدام Google ID Token
    /// </summary>
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}
