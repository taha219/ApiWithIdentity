using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Identity_Roles_API.DTO
{
    public class RegsiterUserDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }

        public string role { get; set; }

        public IFormFile? ProfilePicture { get; set; } 
    }
}
