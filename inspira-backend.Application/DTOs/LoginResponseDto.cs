using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.DTOs
{
    public class LoginResponseDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
