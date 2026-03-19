using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = "";

        [StringLength(500)]
        public string Address { get; set; } = "";

        // Unique membership number
        [StringLength(20)]
        public string MembershipNumber { get; set; } = "";

        // When they joined
        public DateTime JoinedDate { get; set; } = DateTime.Now;

        // Membership expiry
        public DateTime MembershipExpiry { get; set; }

        // Is membership active?
        public bool IsActive { get; set; } = true;

        // Maximum books they can borrow at once
        public int MaxBooksAllowed { get; set; } = 3;

        // Total books borrowed (lifetime)
        public int TotalBooksBorrowed { get; set; }

        // Date of birth
        public DateTime? DateOfBirth { get; set; }

        // Member type (Regular, Premium, Student)
        [StringLength(20)]
        public string MembershipType { get; set; } = "Regular";

        // Navigation property
        public List<BorrowRecord> BorrowRecords { get; set; }

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";

        public bool IsMembershipValid => IsActive && MembershipExpiry > DateTime.Now;

        public int CurrentBorrowedBooks => BorrowRecords?
            .Count(b => b.ReturnDate == null) ?? 0;
    }
}