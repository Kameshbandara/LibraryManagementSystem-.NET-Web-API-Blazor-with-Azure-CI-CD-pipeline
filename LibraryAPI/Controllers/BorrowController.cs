using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Models;
using LibraryAPI.Services;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        private readonly IBorrowService _borrowService;

        public BorrowController(IBorrowService borrowService)
        {
            _borrowService = borrowService;
        }

        // POST: api/borrow
        // Body: { "memberId": 1, "bookId": 2, "daysToReturn": 14 }
        [HttpPost]
        public async Task<ActionResult<BorrowRecord>> BorrowBook([FromBody] BorrowRequest request)
        {
            var result = await _borrowService.BorrowBookAsync(
                request.MemberId,
                request.BookId,
                request.DaysToReturn
            );

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(result.Data);
        }

        // POST: api/borrow/5/return
        [HttpPost("{borrowRecordId}/return")]
        public async Task<ActionResult<BorrowRecord>> ReturnBook(int borrowRecordId)
        {
            var result = await _borrowService.ReturnBookAsync(borrowRecordId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(new
            {
                message = result.Message,
                data = result.Data
            });
        }

        // GET: api/borrow/member/5/history
        [HttpGet("member/{memberId}/history")]
        public async Task<ActionResult<List<BorrowRecord>>> GetMemberHistory(int memberId)
        {
            var records = await _borrowService.GetMemberBorrowHistoryAsync(memberId);
            return Ok(records);
        }

        // GET: api/borrow/active
        [HttpGet("active")]
        public async Task<ActionResult<List<BorrowRecord>>> GetActiveBorrows()
        {
            var records = await _borrowService.GetActiveBorrowsAsync();
            return Ok(records);
        }

        // GET: api/borrow/overdue
        [HttpGet("overdue")]
        public async Task<ActionResult<List<BorrowRecord>>> GetOverdueBorrows()
        {
            var records = await _borrowService.GetOverdueBorrowsAsync();
            return Ok(records);
        }

        // GET: api/borrow/5/fine
        [HttpGet("{borrowRecordId}/fine")]
        public async Task<ActionResult> CalculateFine(int borrowRecordId)
        {
            var result = await _borrowService.CalculateFineAsync(borrowRecordId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }

    // Request DTOs
    public class BorrowRequest
    {
        public int MemberId { get; set; }
        public int BookId { get; set; }
        public int DaysToReturn { get; set; } = 14;
    }
}