using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookieBooksApi.Models;

namespace WookieBooksApi.Controllers
{
    [Authorize(Roles = "Wookie")]
    [Route("api/[controller]")]
    [ApiController]
    [FormatFilter]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserServices _userServices;


        public BooksController(AppDbContext context, UserServices userServices)
        {
            _context = context;
            _userServices = userServices;
        }

        // GET: api/Books
        [AllowAnonymous]
        [HttpGet]
        public  ActionResult<IEnumerable<Books>> GetBooks()
        {
            var results =  (from books in _context.Books
                           join author in _context.Authors on books.AuthorId equals author.AuthorId
                           where books.BookPublished == true
                           select new BooksDetailsDTO
                           {
                               bookId = books.BookId,
                               title = books.Title,
                               price = books.Price,
                               coverImage = books.CoverImage,
                               description = books.Description,
                               authorName = author.AuthorName,
                               authorPseudonym = author.AuthorPseudonym
                           }).ToList();

            return Ok(results);
        }

        // GET: api/Books/5
        [AllowAnonymous]
        [HttpGet("{id}.{format?}")]
        public async Task<ActionResult<BooksDetailsDTO>> GetBooksById(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound(new { message = "Book doesnt exists.!" });
            }

            _context.Entry(book).Reference(x => x.Author).Load();

            var booksDetails = new BooksDetailsDTO()
            {
                bookId = book.BookId,
                title = book.Title,
                price = book.Price,
                coverImage = book.CoverImage,
                description = book.Description,
                authorName = book.Author.AuthorName,
                authorPseudonym = book.Author.AuthorPseudonym
            };
            
            return Ok(booksDetails);
        }

        // GET: 
        [AllowAnonymous]
        [HttpGet("search")]
        public ActionResult<BooksDetailsDTO> SearchBooks([FromQuery] string title, [FromQuery] string authorName)
        {

            var results = _context.Books
                          .Include(x=> x.Author)
                          .Where(b => (b.Title.Contains(title) || title == null) && (b.Author.AuthorName.Contains(authorName) || authorName == null))
                          .Select(books => new
                           {
                               bookId = books.BookId,
                               title = books.Title,
                               price = books.Price,
                               coverImage = books.CoverImage,
                               description = books.Description,
                               authorName = books.Author.AuthorName,
                               authorPseudonym = books.Author.AuthorPseudonym
                           }
                       ).ToList();

            return Ok(results);
        }

        // GET:
        [HttpGet("authorBooks/{userId}")]
        [Authorize]
        public IActionResult GetBooksOfAuthor(int userId)
        {
            string authHeader = Request.Headers["Authorization"];

            if (!_userServices.ValidateRequest(authHeader, userId))
            {
                return BadRequest(new { message = "Request Not Allowed" });
            }

            var result = _context.Books
                             .Where(books => books.Author.UserId == userId)
                             .ToList();
            return Ok(result);
        }

        // POST: api/Books
        [HttpPost]
        public async Task<ActionResult<BooksDetailsDTO>> PostBooks(Books book)
        {
            string authHeader = Request.Headers["Authorization"];

            if (!_userServices.ValidateRequest(authHeader, book.AuthorId))
            {
                return BadRequest(new { message = "Request Not Allowed" });
            }

            if (string.IsNullOrEmpty(book.Title) || book.Title.Length == 0 || book.Title.Equals(" ") || book.Price == 0 || book.Price <= 0 )
            {
                return BadRequest(new { message = "Please enter valid Details. Some Fields are missing..!" });
            }

            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var author = await _context.Authors.SingleOrDefaultAsync(x=> x.UserId == book.AuthorId);
            Books newBook;
            if (author != null)
            {
                newBook = new Books
                {
                    AuthorId = author.AuthorId,
                    Title = book.Title,
                    CoverImage = book.CoverImage,
                    Description = book.Description,
                    Price = book.Price,
                    BookPublished = book.BookPublished ? true : book.BookPublished
                };

                _context.Books.Add(newBook);
                await _context.SaveChangesAsync();

                int bookid = book.BookId;
            }
            else
            {
                return NotFound(new { message = "Author Doesnt Exists!" });
            }

            _context.Entry(newBook).GetDatabaseValues();

            var booksDetails = new
            {
                bookId = newBook.BookId,
                title = newBook.Title,
                price = newBook.Price,
                coverImage = newBook.CoverImage,
                description = newBook.Description,
                authorName = newBook.Author.AuthorName,
                authorPseudonym = newBook.Author.AuthorPseudonym,
                bookPublished = newBook.BookPublished
            };


            return Ok(new { message = "Book Added Successfully!", data = booksDetails });
        }

        // PUT: api/Books/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooks(int id, Books book)
        {
            string authHeader = Request.Headers["Authorization"];

            if (!_userServices.ValidateRequest(authHeader, book.AuthorId))
            {
                return BadRequest(new { message = "Request Not Allowed" });
            }

            if (id != book.BookId)
            {
                return BadRequest();
            }

            try
            {
                var author = await _context.Authors.SingleOrDefaultAsync(x => x.UserId == book.AuthorId);

                var updateRec = new Books
                {
                    BookId = book.BookId,
                    AuthorId = author.AuthorId,
                    Title = book.Title,
                    CoverImage = book.CoverImage,
                    Description = book.Description,
                    Price = book.Price,
                    BookPublished = book.BookPublished
                };

                _context.Entry(updateRec).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException) //Deal Save changes Error
            {
                if (!BooksExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(403, new { message = "Invalid Request" });
                }
            }
            
            return Ok(new { message = "Book Record Updated Successfully..!" });
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Books>> DeleteBooks(int id)
        {
            var book = await _context.Books.FindAsync(id);
            
            if (book == null)
            {
                return NotFound(new { message = "No such Book found..!" });
            }

            try
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.ToString() });
            }

            return Ok(new { message = "Book record Deleted Successfully..!" });
        }

        // DELETE
        [HttpDelete("unpublishbook/{authorUId}/{bookid}")]
        public async Task<ActionResult<Books>> PublishUnpublishBook([FromRoute] int authorUId, [FromRoute] int bookid)
        {
            string authHeader = Request.Headers["Authorization"];

            if (!_userServices.ValidateRequest(authHeader, authorUId))
            {
                return BadRequest(new { message = "Request Not Allowed" });
            }

            var books = _context.Books.FirstOrDefault
                (b => b.BookId == bookid && b.Author.UserId == authorUId);

            if (books != null)
            {
                books.BookPublished = books.BookPublished = false;
                _context.Books.Update(books);
            }
            else
            {
                return NotFound(new { message = "No Record Found..!" });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.ToString() });
            }

            return Ok(new { message = "Book Unpublish Successfuly." });
        }

        private bool BooksExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}
