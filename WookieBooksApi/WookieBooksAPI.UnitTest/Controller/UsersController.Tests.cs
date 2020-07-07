using System;
using System.Collections.Generic;
using System.Text;
using WookieBooksAPI.UnitTest.Models;
using Xunit;
using WookieBooksApi.Controllers;
using Microsoft.Extensions.Options;
using WookieBooksApi.Helpers;
using WookieBooksApi;
using WookieBooksApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WookieBooksAPI.UnitTest.Controller
{
    public class UsersControllerTests
    {
        [Fact]
        public void Login_WhenCalledWith_ValidUsernameAndPassword_ReturnsOkResult()
        {
            // Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext();
            var userServices = new UserServices(GetAppSettings());
            var usersController = new UsersController(dbContext, userServices);

            var mockUser = new User { UserName = "john", Password = "123456" };

            // Act
            var queryRes = usersController.Login(mockUser).Result;

            var data = queryRes.Result as OkObjectResult;

            var responseData = data.Value as ApiResponse;

            dbContext.Dispose();

            // Assert
            Assert.IsType<OkObjectResult>(queryRes.Result);
            Assert.Equal(StatusCodes.Status200OK, data.StatusCode);
            Assert.NotNull(responseData.Token);
        }

        [Fact]
        public void Login_WhenCalledWith_UsernameAndEmptyPassword_ReturnsBadRequest()
        {
            // Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext();
            var userServices = new UserServices(GetAppSettings());
            var usersController = new UsersController(dbContext, userServices);

            var mockUser = new User { UserName = "john", Password = "" };

            // Act
            var queryRes = usersController.Login(mockUser).Result;

            var data = queryRes.Result as ObjectResult;

            dbContext.Dispose();

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, data.StatusCode);
        }

        [Fact]
        public void Login_WhenCalledWith_OnlyPassword_ReturnsBadRequest()
        {
            // Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext();
            var userServices = new UserServices(GetAppSettings());
            var usersController = new UsersController(dbContext, userServices);

            var mockUser = new User { UserName = "", Password = "123456" };

            // Act
            var queryRes = usersController.Login(mockUser).Result;

            var data = queryRes.Result as ObjectResult;

            dbContext.Dispose();

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, data.StatusCode);
        }

        [Fact]
        public void Login_WhenCalledWith_InvalidUsername_ReturnsBadRequest()
        {
            // Arrange
            var dbContext = DbContextMocker.WookieBooksImportersDbContext();
            var userServices = new UserServices(GetAppSettings());
            var usersController = new UsersController(dbContext, userServices);

            var mockUser = new User { UserName = "Tod", Password = "123456" };

            // Act
            var queryRes = usersController.Login(mockUser).Result;

            var data = queryRes.Result as ObjectResult;

            dbContext.Dispose();

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, data.StatusCode);
        }




        public IOptions<AppSettings> GetAppSettings()
        {
            var settings = new AppSettings()
            {
                SecretKey = "WookieBooks12345"
            };
            IOptions<AppSettings> appSettingsOptions = Options.Create(settings);
            return appSettingsOptions;
        }
    }
}
