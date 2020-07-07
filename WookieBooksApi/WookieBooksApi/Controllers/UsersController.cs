using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookieBooksApi.Helpers;
using WookieBooksApi.Models;

namespace WookieBooksApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserServices _userServices;

        public UsersController(AppDbContext context, UserServices userServices)
        {
            _context = context;
            _userServices = userServices;
        }


        // POST: api/Users/Register
        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<User>> Register([FromBody] User user)
        {
            if (string.IsNullOrEmpty(user.UserName) ||
                string.IsNullOrEmpty(user.Name) ||
                string.IsNullOrEmpty(user.Password))
            {
                return BadRequest(new { message = "Please enter valid Details. Some Fields are missing..!" });
            }

            var checkUserExist = _context.Users.FirstOrDefault(x => x.UserName == user.UserName);

            if (checkUserExist == null)
            {

                var passwordHasher = new PasswordHasher<User>();
                var encryptPassword = passwordHasher.HashPassword(user, user.Password);

                var result = _context.Users.Add(new User
                {
                    UserName = user.UserName,
                    Password = encryptPassword,
                    Name = user.Name
                });
                await _context.SaveChangesAsync();

                Author author = new Author { AuthorName = user.Name, UserId = result.Entity.UserId };
                _context.Authors.Add(author);
                
                _context.UserRoleMappings.Add(new UserRoleMapping { UserId = result.Entity.UserId });

                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest(new { message = "Username Already Exists." });
            }

            return Ok(new { message = "User Registered Successfully!" });
        }

        // POST: api/Users/Login
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] User user)
        {
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest(new { message = "Enter Valid Username and password.!" });
            }

            var existingUser = await _context.Users.SingleOrDefaultAsync(u=> u.UserName == user.UserName);

            if (existingUser != null)
            {
                var response = _userServices.Authenticate(existingUser, user, _context);
                if (response == null)
                    return BadRequest(new { message = "Username or password is invalid.!" });
                else
                    return Ok(response);
            }

            return BadRequest(new { message = "Username Does not Exists.!" });
        }
    }
}
