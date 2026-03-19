using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET ALL BOOKS
        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        // GET BOOK BY ID
        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        // CREATE BOOK
        public async Task<ServiceResult<Book>> CreateBookAsync(Book book)
        {
            // Validation 1: Check if ISBN already exists
            var existingBook = await _context.Books
                .FirstOrDefaultAsync(b => b.ISBN == book.ISBN);

            if (existingBook != null)
            {
                return ServiceResult<Book>.FailureResult(
                    "Book with this ISBN already exists",
                    new List<string> { $"ISBN {book.ISBN} is already registered" }
                );
            }

            // Validation 2: Available copies cannot exceed total copies
            if (book.AvailableCopies > book.TotalCopies)
            {
                return ServiceResult<Book>.FailureResult(
                    "Invalid copy count",
                    new List<string> { "Available copies cannot exceed total copies" }
                );
            }

            // Validation 3: Check published year
            if (book.PublishedYear > DateTime.Now.Year)
            {
                return ServiceResult<Book>.FailureResult(
                    "Invalid published year",
                    new List<string> { "Published year cannot be in the future" }
                );
            }

            try
            {
                book.AddedDate = DateTime.Now;
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                return ServiceResult<Book>.SuccessResult(
                    book,
                    "Book added successfully"
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<Book>.FailureResult(
                    "Failed to add book",
                    new List<string> { ex.Message }
                );
            }
        }

        // UPDATE BOOK
        public async Task<ServiceResult<Book>> UpdateBookAsync(int id, Book book)
        {
            var existingBook = await _context.Books.FindAsync(id);

            if (existingBook == null)
            {
                return ServiceResult<Book>.FailureResult(
                    "Book not found",
                    new List<string> { $"Book with ID {id} does not exist" }
                );
            }

            // Check if ISBN is being changed to one that already exists
            if (existingBook.ISBN != book.ISBN)
            {
                var duplicateISBN = await _context.Books
                    .AnyAsync(b => b.ISBN == book.ISBN && b.Id != id);

                if (duplicateISBN)
                {
                    return ServiceResult<Book>.FailureResult(
                        "ISBN already exists",
                        new List<string> { "Another book with this ISBN already exists" }
                    );
                }
            }

            // Validation: Available copies logic
            if (book.AvailableCopies > book.TotalCopies)
            {
                return ServiceResult<Book>.FailureResult(
                    "Invalid copy count",
                    new List<string> { "Available copies cannot exceed total copies" }
                );
            }

            try
            {
                // Update properties
                existingBook.Title = book.Title;
                existingBook.Author = book.Author;
                existingBook.ISBN = book.ISBN;
                existingBook.PublishedYear = book.PublishedYear;
                existingBook.TotalCopies = book.TotalCopies;
                existingBook.AvailableCopies = book.AvailableCopies;
                existingBook.Category = book.Category;

                await _context.SaveChangesAsync();

                return ServiceResult<Book>.SuccessResult(
                    existingBook,
                    "Book updated successfully"
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<Book>.FailureResult(
                    "Failed to update book",
                    new List<string> { ex.Message }
                );
            }
        }

        // DELETE BOOK
        public async Task<ServiceResult> DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return ServiceResult.FailureResult(
                    "Book not found",
                    new List<string> { $"Book with ID {id} does not exist" }
                );
            }

            // Business Rule: Cannot delete if books are currently borrowed
            if (book.AvailableCopies < book.TotalCopies)
            {
                return ServiceResult.FailureResult(
                    "Cannot delete book",
                    new List<string> { "Some copies are currently borrowed. Wait for all returns." }
                );
            }

            try
            {
                // Clean up any historical borrow records to satisfy SQL foreign key constraints
                var historicalRecords = await _context.BorrowRecords.Where(br => br.BookId == id).ToListAsync();
                if (historicalRecords.Any())
                {
                    _context.BorrowRecords.RemoveRange(historicalRecords);
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return ServiceResult.SuccessResult("Book deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Failed to delete book",
                    new List<string> { ex.Message }
                );
            }
        }

        // SEARCH BOOKS
        public async Task<List<Book>> SearchBooksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllBooksAsync();
            }

            return await _context.Books
                .Where(b => b.Title.Contains(searchTerm) ||
                           b.Author.Contains(searchTerm) ||
                           b.ISBN.Contains(searchTerm))
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        // GET AVAILABLE BOOKS
        public async Task<List<Book>> GetAvailableBooksAsync()
        {
            return await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        // BORROW BOOK
        public async Task<ServiceResult> BorrowBookAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);

            if (book == null)
            {
                return ServiceResult.FailureResult(
                    "Book not found",
                    new List<string> { $"Book with ID {bookId} does not exist" }
                );
            }

            // Business Rule: Check availability
            if (book.AvailableCopies <= 0)
            {
                return ServiceResult.FailureResult(
                    "Book not available",
                    new List<string> { "All copies are currently borrowed" }
                );
            }

            try
            {
                book.AvailableCopies--;
                await _context.SaveChangesAsync();

                return ServiceResult.SuccessResult(
                    $"Book borrowed successfully. {book.AvailableCopies} copies remaining."
                );
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Failed to borrow book",
                    new List<string> { ex.Message }
                );
            }
        }

        // RETURN BOOK
        public async Task<ServiceResult> ReturnBookAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);

            if (book == null)
            {
                return ServiceResult.FailureResult(
                    "Book not found",
                    new List<string> { $"Book with ID {bookId} does not exist" }
                );
            }

            // Business Rule: Cannot return more than total copies
            if (book.AvailableCopies >= book.TotalCopies)
            {
                return ServiceResult.FailureResult(
                    "Invalid return",
                    new List<string> { "All copies are already in the library" }
                );
            }

            try
            {
                book.AvailableCopies++;
                await _context.SaveChangesAsync();

                return ServiceResult.SuccessResult(
                    $"Book returned successfully. {book.AvailableCopies} copies now available."
                );
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Failed to return book",
                    new List<string> { ex.Message }
                );
            }
        }
    }
}