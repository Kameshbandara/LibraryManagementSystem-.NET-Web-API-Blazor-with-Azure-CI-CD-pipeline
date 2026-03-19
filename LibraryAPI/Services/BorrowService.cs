using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    public class BorrowService : IBorrowService
    {
        private readonly ApplicationDbContext _context;
        private const decimal FINE_PER_DAY = 50; // Rs 50 per day

        public BorrowService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<BorrowRecord>> BorrowBookAsync(
            int memberId,
            int bookId,
            int daysToReturn = 14)
        {
            // Check member exists and is active
            var member = await _context.Members.FindAsync(memberId);
            if (member == null)
            {
                return ServiceResult<BorrowRecord>.FailureResult("Member not found");
            }

            if (!member.IsActive)
            {
                return ServiceResult<BorrowRecord>.FailureResult("Member account is inactive");
            }

            if (member.MembershipExpiry < DateTime.Now)
            {
                return ServiceResult<BorrowRecord>.FailureResult("Membership has expired");
            }

            // Check book exists and is available
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return ServiceResult<BorrowRecord>.FailureResult("Book not found");
            }

            if (book.AvailableCopies <= 0)
            {
                return ServiceResult<BorrowRecord>.FailureResult("Book is not available");
            }

            // Check member's current borrowed books count
            var currentBorrows = await _context.BorrowRecords
                .CountAsync(br => br.MemberId == memberId && br.ReturnDate == null);

            if (currentBorrows >= member.MaxBooksAllowed)
            {
                return ServiceResult<BorrowRecord>.FailureResult(
                    $"Member has reached maximum borrow limit ({member.MaxBooksAllowed} books)"
                );
            }

            // Check if member already has this book borrowed
            var alreadyBorrowed = await _context.BorrowRecords
                .AnyAsync(br => br.MemberId == memberId &&
                              br.BookId == bookId &&
                              br.ReturnDate == null);

            if (alreadyBorrowed)
            {
                return ServiceResult<BorrowRecord>.FailureResult(
                    "Member already has this book borrowed"
                );
            }

            // Create borrow record
            var borrowRecord = new BorrowRecord
            {
                MemberId = memberId,
                BookId = bookId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(daysToReturn),
                Status = "Borrowed"
            };

            try
            {
                // Add borrow record
                _context.BorrowRecords.Add(borrowRecord);

                // Update book availability
                book.AvailableCopies--;
                book.TimesBorrowed++;

                // Update member statistics
                member.TotalBooksBorrowed++;

                await _context.SaveChangesAsync();

                // Load navigation properties for response
                await _context.Entry(borrowRecord)
                    .Reference(br => br.Book)
                    .LoadAsync();
                await _context.Entry(borrowRecord)
                    .Reference(br => br.Member)
                    .LoadAsync();

                return ServiceResult<BorrowRecord>.SuccessResult(
                    borrowRecord,
                    $"Book borrowed successfully. Due date: {borrowRecord.DueDate:yyyy-MM-dd}"
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<BorrowRecord>.FailureResult(
                    "Failed to borrow book",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ServiceResult<BorrowRecord>> ReturnBookAsync(int borrowRecordId)
        {
            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.Member)
                .FirstOrDefaultAsync(br => br.Id == borrowRecordId);

            if (borrowRecord == null)
            {
                return ServiceResult<BorrowRecord>.FailureResult("Borrow record not found");
            }

            if (borrowRecord.ReturnDate != null)
            {
                return ServiceResult<BorrowRecord>.FailureResult("Book already returned");
            }

            try
            {
                // Set return date
                borrowRecord.ReturnDate = DateTime.Now;
                borrowRecord.Status = "Returned";

                // Calculate fine if overdue
                if (DateTime.Now > borrowRecord.DueDate)
                {
                    var daysOverdue = (DateTime.Now - borrowRecord.DueDate).Days;
                    borrowRecord.FineAmount = daysOverdue * FINE_PER_DAY;
                    borrowRecord.Status = "Returned (Overdue)";
                }

                // Update book availability
                borrowRecord.Book.AvailableCopies++;

                await _context.SaveChangesAsync();

                var message = borrowRecord.FineAmount > 0
                    ? $"Book returned. Fine amount: ${borrowRecord.FineAmount}"
                    : "Book returned successfully";

                return ServiceResult<BorrowRecord>.SuccessResult(borrowRecord, message);
            }
            catch (Exception ex)
            {
                return ServiceResult<BorrowRecord>.FailureResult(
                    "Failed to return book",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<List<BorrowRecord>> GetMemberBorrowHistoryAsync(int memberId)
        {
            return await _context.BorrowRecords
                .Include(br => br.Book)
                .Where(br => br.MemberId == memberId)
                .OrderByDescending(br => br.BorrowDate)
                .ToListAsync();
        }

        public async Task<List<BorrowRecord>> GetActiveBorrowsAsync()
        {
            return await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.Member)
                .Where(br => br.ReturnDate == null)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }

        public async Task<List<BorrowRecord>> GetOverdueBorrowsAsync()
        {
            return await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.Member)
                .Where(br => br.ReturnDate == null && br.DueDate < DateTime.Now)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }

        public async Task<ServiceResult> CalculateFineAsync(int borrowRecordId)
        {
            var borrowRecord = await _context.BorrowRecords.FindAsync(borrowRecordId);

            if (borrowRecord == null)
            {
                return ServiceResult.FailureResult("Borrow record not found");
            }

            if (borrowRecord.ReturnDate != null)
            {
                return ServiceResult.SuccessResult($"Book returned. Fine: ${borrowRecord.FineAmount}");
            }

            if (DateTime.Now <= borrowRecord.DueDate)
            {
                return ServiceResult.SuccessResult("No fine. Book is not overdue.");
            }

            var daysOverdue = (DateTime.Now - borrowRecord.DueDate).Days;
            var fineAmount = daysOverdue * FINE_PER_DAY;

            return ServiceResult.SuccessResult(
                $"Book is {daysOverdue} days overdue. Fine amount: ${fineAmount}"
            );
        }
    }
}