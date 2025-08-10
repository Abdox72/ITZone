using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace platform_backend.Models
{
    /// <summary>
    /// نموذج Refresh Token لإدارة الجلسات طويلة المدى
    /// </summary>
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        public string? UserAgent { get; set; }

        public string? IpAddress { get; set; }

        /// <summary>
        /// التحقق من صحة التوكن
        /// </summary>
        public bool IsValid => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

        /// <summary>
        /// إلغاء التوكن
        /// </summary>
        public void Revoke()
        {
            RevokedAt = DateTime.UtcNow;
        }
    }
}