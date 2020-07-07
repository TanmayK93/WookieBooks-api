using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using WookieBooksApi.Controllers;
using WookieBooksApi.Models;
using WookieBooksAPI.UnitTest.Models;
using Xunit;

namespace WookieBooksAPI.UnitTest.Controller
{
    public class BooksControllerTests
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

            // Assert
            var record = Assert.IsType<OkObjectResult>(okResult.Result.Result);
            Assert.Equal(bookid, (record.Value as BooksDetailsDTO).bookId);
        }

    }
}
