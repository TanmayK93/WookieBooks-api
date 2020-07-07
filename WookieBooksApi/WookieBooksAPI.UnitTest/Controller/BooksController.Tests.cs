using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WookieBooksApi.Controllers;
using WookieBooksApi.Models;
using WookieBooksAPI.UnitTest.MockData;
using WookieBooksAPI.UnitTest.Models;
using Xunit;

namespace WookieBooksAPI.UnitTest.Controller
{
    public class BooksControllerTests : BookTestData
    {

        [Fact]
        public void GetBooks_WhenCalled_ReturnsOkResult()
        {
            // Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext(nameof(GetBooks_WhenCalled_ReturnsOkResult));
            var booksController = new BooksController(dbContext, null);

            // Act
            var response = booksController.GetBooks();

            dbContext.Dispose();

            // Assert
            Assert.IsType<OkObjectResult>(response.Result);
        }

        [Fact]
        public void GetBooks_WhenCalled_ReturnsAllItems()
        {
            //Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext(nameof(GetBooks_WhenCalled_ReturnsAllItems));
            var booksController = new BooksController(dbContext, null);

            // Act
            var okResult = booksController.GetBooks().Result as OkObjectResult;
            dbContext.Dispose();

            // Assert
            var items = Assert.IsType<List<BooksDetailsDTO>>(okResult.Value);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public void GetBooksById_UnknownInValidbookIdPassed_ReturnsNotFoundObjectResult()
        {
            //Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext(nameof(GetBooksById_UnknownInValidbookIdPassed_ReturnsNotFoundObjectResult));
            var booksController = new BooksController(dbContext, null);
            int bookid = 10;


            // Act
            var notFoundResult = booksController.GetBooksById(bookid);
            dbContext.Dispose();

            // Assert
            Assert.IsType<NotFoundObjectResult>(notFoundResult.Result.Result);
        }

        [Fact]
        public void GetBooksById_ExistingValidbookId1Passed_ReturnsOkResult()
        {
            //Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext(nameof(GetBooksById_ExistingValidbookId1Passed_ReturnsOkResult));
            var booksController = new BooksController(dbContext, null);
            int bookid = 1;
            

            // Act
            var okResult = booksController.GetBooksById(bookid);
            dbContext.Dispose();

            // Assert
            Assert.IsType<OkObjectResult>(okResult.Result.Result);
        }

        [Fact]
        public void GetBooksById_ExistingIdPassed1_ReturnsBookInformationWithId1()
        {
            // Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext(nameof(GetBooksById_ExistingIdPassed1_ReturnsBookInformationWithId1));
            var booksController = new BooksController(dbContext, null);
            int bookid = 1;

            // Act
            var okResult = booksController.GetBooksById(bookid);
            dbContext.Dispose();

            // Assert
            var record = Assert.IsType<OkObjectResult>(okResult.Result.Result);
            Assert.Equal(bookid, (record.Value as BooksDetailsDTO).bookId);
        }


        [Theory]
        [ClassData(typeof(BookTestData))]
        public void CreateBooks_WithNoTitle_OR_EmptyTitle_OR_EmptyPrice_OR_NegativePrice_ReturnStatusCode400(Books book)
        {

            //Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext(nameof(CreateBooks_WithNoTitle_OR_EmptyTitle_OR_EmptyPrice_OR_NegativePrice_ReturnStatusCode400));
            var booksController = new BooksController(dbContext, null);
            

            // Act
            var result =  booksController.PostBooks(book);
            dbContext.Database.EnsureDeleted();

            // Assert
            var record = result.Result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status400BadRequest, record.StatusCode);
        }

        [Fact]
        public void CreateBooks_WithValidTitlePriceAndInvalidAuthor_ReturnStatusCode404()
        {

            //Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext(nameof(CreateBooks_WithValidTitlePriceAndInvalidAuthor_ReturnStatusCode404));
            var booksController = new BooksController(dbContext, null);

            var mockData = new Books
            {
                BookId = 3,
                Title = "Book 3",
                AuthorId = 3,
                Price = 12.2m,
                CoverImage = "data/base64",
                BookPublished = true,
                Description = "This is Book 3"
            };


            // Act
            var result = booksController.PostBooks(mockData);
            dbContext.Dispose();

            // Assert
            var record = result.Result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, record.StatusCode);
        }

        [Fact]
        public void CreateBooks_WithValidTitlePriceAndValidAuthor_ReturnStatusCode200()
        {

            //Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext(nameof(CreateBooks_WithValidTitlePriceAndValidAuthor_ReturnStatusCode200));
            var booksController = new BooksController(dbContext, null);

            var mockData = new Books
            {
                BookId = 3,
                Title = "Book 3",
                AuthorId = 1,
                Price = 12.2m,
                CoverImage = "data/base64",
                BookPublished = true,
                Description = "This is Book 3"
            };


            // Act
            var result = booksController.PostBooks(mockData);
            dbContext.Dispose();

            // Assert
            var record = result.Result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status200OK, record.StatusCode);
        }
    }
    
}
