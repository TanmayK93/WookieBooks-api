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
            //Only Published Books Will be shown to the end users.
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

        /// <summary>
        /// Search Books Based on ID.
        /// 1. If found record the detail record of book.
        /// 2. Else Return message with book doesnt exist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

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
        /// <summary>
        /// Search Books by title or wuthor name
        /// 1. If not found return empty object
        /// </summary>
        /// <param name="title"></param>
        /// <param name="authorName"></param>
        /// <returns></returns>

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

        /// <summary>
        /// Return books for the login author. 
        /// Only Books of login in user will be return. If not found return empty object
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create a new Book by validating below conditions.
        /// 1. Login author should be same received userid from HTTP request.
        /// 2. Check for empty field. 
        /// 3. Authorid details should match in exisiting system.
        /// 4. If all the above details are valid return details of book.
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>

        // POST: api/Books
        [HttpPost]
        public async Task<ActionResult<BooksDetailsDTO>> PostBooks(Books book)
        {
            /* The code is used in the books controller for additional validation at server side. 
             * Which would authenticate the request based on userId send from route and token. 
             * If it matches then request will be processed 
             */
            
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

        /// <summary>
        /// This route is used to update the book record for the logged author.
        /// if valid details are send record will be updated
        /// </summary>
        /// <param name="id"></param>
        /// <param name="book"></param>
        /// <returns></returns>

        // PUT: api/Books/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooks(int id, [FromBody] Books book)
        {
            /* The code is used in the books controller for additional validation at server side. 
            * Which would authenticate the request based on userId send from route and token. 
            * If it matches then request will be processed 
            */

            string authHeader = Request.Headers["Authorization"];

            if (!_userServices.ValidateRequest(authHeader, book.AuthorId))
            {
                return BadRequest(new { message = "Request Not Allowed" });
            }


            if (id != book.BookId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Bug Fix Unit Testing.
                var existing = await _context.Books.FindAsync(book.BookId);
                if (existing != null)
                {
                    existing.Title = book.Title;
                    existing.CoverImage = book.CoverImage;
                    existing.Description = book.Description;
                    existing.Price = book.Price;
                    existing.BookPublished = book.BookPublished;

                    _context.Entry(existing).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException) //Deal Save changes Error
            {
                if (!BooksExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid Request" });
                }
            }
            
            return Ok(new { message = "Book Record Updated Successfully..!" });
        }

        /// <summary>
        /// This route is used to delete book record based on id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

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

        /// <summary>
        /// This route is used to unpublish a book.
        /// 1. First Validation will be done of userid received in route and auth header if details are valid then request is processed.
        /// </summary>
        /// <param name="authorUId"></param>
        /// <param name="bookid"></param>
        /// <returns></returns>

        // DELETE
        [HttpDelete("unpublishbook/{authorUId}/{bookid}")]
        public async Task<ActionResult<Books>> PublishUnpublishBook([FromRoute] int authorUId, [FromRoute] int bookid)
        {
            /* The code is used in the books controller for additional validation at server side. 
            * Which would authenticate the request based on userId send from route and token. 
            * If it matches then request will be processed 
            */

            string authHeader = Request.Headers["Authorization"];

            if (!_userServices.ValidateRequest(authHeader, authorUId))
            {
                return BadRequest(new { message = "Request Not Allowed" });
            }

            //Check for the book details
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
