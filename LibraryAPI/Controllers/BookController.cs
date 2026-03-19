using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Models;
using LibraryAPI.Services;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET: api/books
        [HttpGet]
        public async Task<ActionResult<List<Book>>> GetAllBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        // GET: api/books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound(new { message = $"Book with ID {id} not found" });

            return Ok(book);
        }

        // POST: api/books
        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook([FromBody] Book book)
        {
            var result = await _bookService.CreateBookAsync(book);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return CreatedAtAction(nameof(GetBook), new { id = result.Data.Id }, result.Data);
        }

        // PUT: api/books/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Book>> UpdateBook(int id, [FromBody] Book book)
        {
            var result = await _bookService.UpdateBookAsync(id, book);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(result.Data);
        }

        // DELETE: api/books/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return NoContent();
        }

        // GET: api/books/search?term=code
        [HttpGet("search")]
        public async Task<ActionResult<List<Book>>> SearchBooks([FromQuery] string term)
        {
            var books = await _bookService.SearchBooksAsync(term);
            return Ok(books);
        }

        // GET: api/books/available
        [HttpGet("available")]
        public async Task<ActionResult<List<Book>>> GetAvailableBooks()
        {
            var books = await _bookService.GetAvailableBooksAsync();
            return Ok(books);
        }

        // POST: api/books/{bookId}/borrow
        [HttpPost("{bookId}/borrow")]
        public async Task<ActionResult> BorrowBook(int bookId)
        {
            var result = await _bookService.BorrowBookAsync(bookId);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message });
        }

        // POST: api/books/{bookId}/return
        [HttpPost("{bookId}/return")]
        public async Task<ActionResult> ReturnBook(int bookId)
        {
            var result = await _bookService.ReturnBookAsync(bookId);

            if (!result.Success)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message });
        }
    }
}
