using LibraryBlazorApp.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryBlazorApp.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }

        // Foreign Keys
        public int MemberId { get; set; }
        public int BookId { get; set; }

        // When borrowed
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        // When due to return
        public DateTime DueDate { get; set; }

        // When actually returned (null if not returned yet)
        public DateTime? ReturnDate { get; set; }

        // Status: "Borrowed", "Returned", "Overdue"
        [StringLength(20)]
        public string Status { get; set; } = "Borrowed";

        // Fine amount (if overdue)
        [Column(TypeName = "decimal(18,2)")]
        public decimal FineAmount { get; set; }

        // Notes (condition of book, etc.)
        [StringLength(500)]
        public string Notes { get; set; } = "";

        // Navigation properties
        public Member Member { get; set; }
        public Book Book { get; set; }

        // Computed properties
        public bool IsOverdue => ReturnDate == null && DateTime.Now > DueDate;

        public int DaysOverdue
        {
            get
            {
                if (ReturnDate != null) return 0;
                if (DateTime.Now <= DueDate) return 0;
                return (DateTime.Now - DueDate).Days;
            }
        }

        public int DaysRemaining
        {
            get
            {
                if (ReturnDate != null) return 0;
                if (DateTime.Now >= DueDate) return 0;
                return (DueDate - DateTime.Now).Days;
            }
        }
    }
}