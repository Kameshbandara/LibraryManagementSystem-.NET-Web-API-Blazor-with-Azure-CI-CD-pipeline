using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = "";

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsRevoked => RevokedAt != null;

        public bool IsActive => !IsRevoked && !IsExpired;

        // Foreign key
        public int UserId { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
