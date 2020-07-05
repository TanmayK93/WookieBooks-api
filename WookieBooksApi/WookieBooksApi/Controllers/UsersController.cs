using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // POST: api/Users/Login
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<Author>> Login([FromBody] User user)
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
