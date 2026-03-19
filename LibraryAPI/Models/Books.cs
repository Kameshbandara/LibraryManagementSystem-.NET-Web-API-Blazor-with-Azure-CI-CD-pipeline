using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Author is required")]
        [StringLength(100)]
        public string Author { get; set; } = "";

        [Required]
        [StringLength(20)]
        public string ISBN { get; set; } = "";

        // ✨ NEW: Description field
        [StringLength(1000)]
        public string Description { get; set; } = "";

        // ✨ NEW: Price field
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000)]
        public decimal Price { get; set; }

        // ✨ NEW: Cover image URL
        [StringLength(500)]
        public string CoverImageUrl { get; set; } = "";

        // ✨ NEW: Publisher
        [StringLength(100)]
        public string Publisher { get; set; } = "";

        // ✨ NEW: Language
        [StringLength(50)]
        public string Language { get; set; } = "English";

        // ✨ NEW: Number of pages
        [Range(1, 10000)]
        public int Pages { get; set; }

        [Range(1800, 2100)]
        public int PublishedYear { get; set; }

        public int TotalCopies { get; set; }

        public int AvailableCopies { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = "";

        // ✨ NEW: Rating (1-5 stars)
        [Range(0, 5)]
        public double Rating { get; set; }

        // ✨ NEW: Is book featured?
        public bool IsFeatured { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;

        // ✨ NEW: Last updated date
        public DateTime? LastUpdatedDate { get; set; }

        // Navigation property for borrowing records
        public List<BorrowRecord> BorrowRecords { get; set; }

        // Computed property
        public bool IsAvailable => AvailableCopies > 0;

        // ✨ NEW: How many times borrowed
        public int TimesBorrowed { get; set; }
    }
}