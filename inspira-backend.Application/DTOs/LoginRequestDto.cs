using System.ComponentModel.DataAnnotations;

namespace inspira_backend.Application.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
