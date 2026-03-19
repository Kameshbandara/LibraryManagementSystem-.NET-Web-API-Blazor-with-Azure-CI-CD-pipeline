using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    // "I" prefix = Interface (naming convention)
    public interface IBookService
    {
        // Method 1: Get all books
        Task<List<Book>> GetAllBooksAsync();

        // Method 2: Get one book by ID
        Task<Book> GetBookByIdAsync(int id);

        // Method 3: Create new book
        Task<ServiceResult<Book>> CreateBookAsync(Book book);

        // Method 4: Update existing book
        Task<ServiceResult<Book>> UpdateBookAsync(int id, Book book);

        // Method 5: Delete book
        Task<ServiceResult> DeleteBookAsync(int id);

        // Method 6: Search books
        Task<List<Book>> SearchBooksAsync(string searchTerm);

        // Method 7: Get available books
        Task<List<Book>> GetAvailableBooksAsync();

        // Method 8: Borrow book
        Task<ServiceResult> BorrowBookAsync(int bookId);

        // Method 9: Return book
        Task<ServiceResult> ReturnBookAsync(int bookId);
    }
}