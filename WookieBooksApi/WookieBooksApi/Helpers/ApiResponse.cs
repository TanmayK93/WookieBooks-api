using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WookieBooksApi.Models;

namespace WookieBooksApi.Helpers
{
    public class ApiResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }

        public ApiResponse(User user, string token)
        {
            
            Username = user.UserName;
            UserId = user.UserId;
            Token = token;
        }
    }
}
