using System;
using System.Collections.Generic;
using System.Text;
using WookieBooksApi.Models;

namespace WookieBooksAPI.UnitTest.Models
{
    public static class DbContextExtensions
    {
        public static void Seed(this AppDbContext _context)
        {
            var authors = new List<Author>
            {
                 new Author {
                    AuthorId = 1,
                    AuthorName = "John",
                    AuthorPseudonym = "Wookie",
                    UserId = 1
                },
                new Author {
                    AuthorId = 2,
                    AuthorName = "Annie",
                    AuthorPseudonym = "Darth Vader",
                    UserId = 2
                },
            };

            var books = new List<Books>
            {
                 new Books {
                    BookId = 1,
                    Title = "Book 1",
                    Price = 20.2m,
                    CoverImage = "data:image/png;base64",
                    Description = "This is Book 1",
                    AuthorId = 1,
                    BookPublished = true
                },
                new Books {
                    BookId = 2,
                    Title = "Book 2",
                    Price = 10.2m,
                    CoverImage = "data:image/png;base64",
                    Description = "This is Book 2",
                    AuthorId = 1,
                    BookPublished = true
                },
            };

            var users = new List<User>
            {
                 new User {
                    UserId = 1,
                    Name = "John",
                    UserName = "john",
                    Password = "AQAAAAEAACcQAAAAEH13j4wZ3TAcrwv/uvgbF6ujqh4UCbDs69mAQpBWTeWF9/swmZ95xNYOgiJLb5pmTg=="
                 },
                new User {
                    UserId = 2,
                    Name = "Annie",
                    UserName = "annie",
                    Password = "AQAAAAEAACcQAAAAEH13j4wZ3TAcrwv/uvgbF6ujqh4UCbDs69mAQpBWTeWF9/swmZ95xNYOgiJLb5pmTg=="
                },
                 new User {
                    UserId = 3,
                    Name = "Admin",
                    UserName = "admin",
                    Password = "AQAAAAEAACcQAAAAEH13j4wZ3TAcrwv/uvgbF6ujqh4UCbDs69mAQpBWTeWF9/swmZ95xNYOgiJLb5pmTg=="
                 },
            };

            var roles = new List<Role>
            {
                new Role
                {
                    RoleId = 1,
                    RoleName = "Admin"
                },
                new Role
                {
                    RoleId = 2,
                    RoleName = "Author"
                }
            };

            var userRoleMappings = new List<UserRoleMapping>
            {
                new UserRoleMapping
                {
                    UserId = 1,
                    RoleId = 2,
                },
                new UserRoleMapping
                {
                    UserId = 2,
                    RoleId = 2,
                },
                //Admin
                new UserRoleMapping
                {
                    UserId = 3,
                    RoleId = 1,
                }
            };

            /*
            _context.Roles.AddRange(roles);
            _context.SaveChanges();
            */

            _context.Users.AddRange(users);
            _context.SaveChanges();

            _context.Authors.AddRange(authors);
            _context.SaveChanges();

            _context.UserRoleMappings.AddRange(userRoleMappings);
            _context.SaveChanges();
           
            _context.Books.AddRange(books);
            _context.SaveChanges();
        }
    }
}
