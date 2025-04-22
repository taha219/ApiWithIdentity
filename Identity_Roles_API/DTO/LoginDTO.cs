using System.ComponentModel.DataAnnotations;

namespace Identity_Roles_API.DTO
{
    public class LoginDTO
    {
        [Required]
          public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
