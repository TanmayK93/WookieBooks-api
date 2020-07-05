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
        public async Task<ActionResult<Books>> GetBooksById(Guid id)
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
                _context.Books.Add(new Books {
                    AuthorId = author.AuthorId,
                    Title = book.Title,
                    CoverImage = book.CoverImage,
                    Description = book.Description,
                    Price = book.Price
                });
                await _context.SaveChangesAsync();
            }
            else
            {
                return NotFound(new { message = "Author Doesnt Exists!" });
            }

            var id = _context.Entry(book).State = EntityState.Added;

            var result =  _context.Entry(book).Entity;
            

            return Ok(new { message = "Book Added Successfully!", data = result });
        }
        
        private bool BooksExists(Guid id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}
