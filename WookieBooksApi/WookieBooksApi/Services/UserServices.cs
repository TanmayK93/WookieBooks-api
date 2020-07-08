using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WookieBooksApi.Helpers;
using WookieBooksApi.Models;

namespace WookieBooksApi
{
    public class UserServices
    {
        private readonly AppSettings _appSettings;

        public UserServices(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// This block of code is used to validate the user password with exisitng password.
        /// If its valid create a token and back to client.
        /// </summary>
        /// <param name="existingUser"></param>
        /// <param name="receivedUser"></param>
        /// <param name="_context"></param>
        /// <returns></returns>
        public ApiResponse Authenticate(User existingUser, User receivedUser, AppDbContext _context)
        {
            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(receivedUser, existingUser.Password, receivedUser.Password);

            if (result == PasswordVerificationResult.Success)
            {
                string token = generateJwtToken(existingUser, _context);
                return new ApiResponse(existingUser, token);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Generate Token for the user and determine type of role[Wookie, Admin]
        /// </summary>
        /// <param name="user"></param>
        /// <param name="_context"></param>
        /// <returns></returns>

        private string generateJwtToken(User user, AppDbContext _context)
        {
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.SecretKey);

            var roleType = _context.UserRoleMappings.Include(x => x.Role).SingleOrDefault(x => x.UserId == user.UserId).Role.RoleName.ToLower();
            
            //_context.UserRoleMappings.SingleOrDefault(x => x.UserId == user.UserId);
            string pseudonym = string.Empty;
            string role = string.Empty;
            if (roleType != "admin")
            {
                pseudonym = _context.Authors.SingleOrDefault(x => x.UserId == user.UserId).AuthorPseudonym.ToLower();
                role = pseudonym.First().ToString().ToUpper() + pseudonym.Substring(1);
            }
            else
            {
                role = roleType.First().ToString().ToUpper() + roleType.Substring(1);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                   new Claim("UserId", user.UserId.ToString()),
                   new Claim("UserName", user.UserName.ToString()),
                   new Claim(ClaimTypes.Role, role.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// This block is used to validate the token information before processing 
        /// GET, POST, PUT and Delete request.
        /// </summary>
        /// <param name="Header"></param>
        /// <param name="userId"></param>
        /// <returns></returns>

        public bool ValidateRequest(string Header, int userId)
        {
            var handler = new JwtSecurityTokenHandler();
            var authHeader = Header.Replace("Bearer ", "");
            var jsonToken = handler.ReadToken(authHeader);
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            var id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
            
            if (int.Parse(id) == userId)
            {
                return true;
            }

            return false;
        }
    }
}