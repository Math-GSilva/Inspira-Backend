using inspira_backend.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace inspira_backend.Application.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string CompleteName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }
    }
}
