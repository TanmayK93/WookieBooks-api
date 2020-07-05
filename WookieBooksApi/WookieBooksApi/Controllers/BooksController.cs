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
    [Route("api/[controller]")]
    [ApiController]
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
        [HttpGet]
        [AllowAnonymous]
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
        [HttpGet("{id}.{format?}")]
        [AllowAnonymous]
        public async Task<ActionResult<Books>> GetBooksById(int id)
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

        // POST: api/Books
        [HttpPost]
        public async Task<ActionResult<Books>> PostBooks(Books book)
        {
            string authHeader = Request.Headers["Authorization"];

            if (!_userServices.ValidateRequest(authHeader, book.AuthorId))
            {
                return BadRequest(new { message = "Request Not Allowed" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var author = await _context.Authors.SingleOrDefaultAsync(x=> x.UserId == book.AuthorId);

            if (author != null)
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
            }
            else
            {
                return NotFound(new { message = "Author Doesnt Exists!" });
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
                _context.Entry(book).State = EntityState.Modified;
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
