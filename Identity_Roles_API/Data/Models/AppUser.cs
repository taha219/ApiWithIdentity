using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Identity_Roles_API.Data.Models
{
    public class AppUser : IdentityUser
    {
        public string role { get; set; }
        public byte[]? ProfilePicture { get; set; }
    }
}
