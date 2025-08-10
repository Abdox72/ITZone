using System.ComponentModel.DataAnnotations;

namespace platform_backend.Models
{
    /// <summary>
    /// نموذج المستخدم في قاعدة البيانات
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string GoogleId { get; set; } = string.Empty; // Google User ID (unique)

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string DisplayName { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public bool EmailVerified { get; set; } = false;

        [Required]
        public string Provider { get; set; } = "Google"; // Keep for consistency

        [Required]
        public string ProviderId { get; set; } = string.Empty; // Same as GoogleId for backward compatibility

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation property for refresh tokens
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}