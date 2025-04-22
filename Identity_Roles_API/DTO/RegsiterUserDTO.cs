using System.ComponentModel.DataAnnotations;

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
    }
}
