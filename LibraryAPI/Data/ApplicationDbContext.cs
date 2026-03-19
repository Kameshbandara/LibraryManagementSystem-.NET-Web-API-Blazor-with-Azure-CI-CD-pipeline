using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LibraryAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets (Tables)
        public DbSet<Book> Books { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ═══════════════════════════════════════
            // BOOK CONFIGURATION
            // ═══════════════════════════════════════
            modelBuilder.Entity<Book>(entity =>
            {
                // ISBN must be unique
                entity.HasIndex(b => b.ISBN).IsUnique();

                // Default values
                entity.Property(b => b.AddedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(b => b.Language)
                    .HasDefaultValue("English");

                // Ignore computed property
                entity.Ignore(b => b.IsAvailable);
            });

            // ═══════════════════════════════════════
            // MEMBER CONFIGURATION
            // ═══════════════════════════════════════
            modelBuilder.Entity<Member>(entity =>
            {
                // Email must be unique
                entity.HasIndex(m => m.Email).IsUnique();

                // Membership number must be unique
                entity.HasIndex(m => m.MembershipNumber).IsUnique();

                // Default values
                entity.Property(m => m.JoinedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(m => m.IsActive)
                    .HasDefaultValue(true);

                entity.Property(m => m.MaxBooksAllowed)
                    .HasDefaultValue(3);

                // Ignore computed properties
                entity.Ignore(m => m.FullName);
                entity.Ignore(m => m.IsMembershipValid);
                entity.Ignore(m => m.CurrentBorrowedBooks);
            });

            // ═══════════════════════════════════════
            // BORROW RECORD CONFIGURATION
            // ═══════════════════════════════════════
            modelBuilder.Entity<BorrowRecord>(entity =>
            {
                // Relationship: BorrowRecord → Member
                entity.HasOne(br => br.Member)
                    .WithMany(m => m.BorrowRecords)
                    .HasForeignKey(br => br.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship: BorrowRecord → Book
                entity.HasOne(br => br.Book)
                    .WithMany(b => b.BorrowRecords)
                    .HasForeignKey(br => br.BookId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Default values
                entity.Property(br => br.BorrowDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(br => br.Status)
                    .HasDefaultValue("Borrowed");

                // Ignore computed properties
                entity.Ignore(br => br.IsOverdue);
                entity.Ignore(br => br.DaysOverdue);
            });

            // ═══════════════════════════════════════
            // SEED DATA
            // ═══════════════════════════════════════

            // Seed Books
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    ISBN = "978-0132350884",
                    Description = "A handbook of agile software craftsmanship",
                    Price = 45.99m,
                    Publisher = "Prentice Hall",
                    Pages = 464,
                    PublishedYear = 2008,
                    TotalCopies = 5,
                    AvailableCopies = 5,
                    Category = "Programming",
                    Rating = 4.5,
                    Language = "English",
                    CoverImageUrl = "https://unsplash.com/photos/a-person-holding-a-book-in-their-hand-enpEQKJfRyU"
                },
                new Book
                {
                    Id = 2,
                    Title = "The Pragmatic Programmer",
                    Author = "Andrew Hunt",
                    ISBN = "978-0201616224",
                    Description = "Your journey to mastery",
                    Price = 49.99m,
                    Publisher = "Addison-Wesley",
                    Pages = 352,
                    PublishedYear = 1999,
                    TotalCopies = 3,
                    AvailableCopies = 3,
                    Category = "Programming",
                    Rating = 4.7,
                    Language = "English",
                    CoverImageUrl = "https://unsplash.com/photos/a-computer-screen-with-a-bunch-of-lines-on-it-aYosQyFcC8k"
                }
            );

            // Seed Members
            modelBuilder.Entity<Member>().HasData(
                new Member
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@email.com",
                    PhoneNumber = "+1234567890",
                    Address = "123 Main St, City",
                    MembershipNumber = "MEM2024001",
                    JoinedDate = new DateTime(2024, 4, 1),
                    MembershipExpiry = new DateTime(2030, 4, 1),
                    IsActive = true,
                    MembershipType = "Premium",
                    MaxBooksAllowed = 5,
                    DateOfBirth = new DateTime(1998, 7, 1)
                },
                new Member
                {
                    Id = 2,
                    FirstName = "Jane",
                    LastName = "Smith",

                    Email = "jane.smith@email.com",
                    PhoneNumber = "+1234567891",
                    Address = "456 Oak Ave, City",
                    MembershipNumber = "MEM2024002",
                    JoinedDate = new DateTime(2024, 7, 1),
                    MembershipExpiry = new DateTime(2030, 7, 1),
                    IsActive = true,
                    MembershipType = "Regular",
                    MaxBooksAllowed = 3,
                    DateOfBirth = new DateTime(2000, 7, 14)
                }
            );
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

    }
}