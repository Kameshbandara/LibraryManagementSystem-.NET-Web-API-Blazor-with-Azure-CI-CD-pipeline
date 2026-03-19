using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    public interface IBorrowService
    {
        Task<ServiceResult<BorrowRecord>> BorrowBookAsync(int memberId, int bookId, int daysToReturn = 14);
        Task<ServiceResult<BorrowRecord>> ReturnBookAsync(int borrowRecordId);
        Task<List<BorrowRecord>> GetMemberBorrowHistoryAsync(int memberId);
        Task<List<BorrowRecord>> GetActiveBorrowsAsync();
        Task<List<BorrowRecord>> GetOverdueBorrowsAsync();
        Task<ServiceResult> CalculateFineAsync(int borrowRecordId);
    }
}