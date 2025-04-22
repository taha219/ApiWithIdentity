using System.ComponentModel.DataAnnotations;
using Identity_Roles_API.Data.Models;

namespace Identity_Roles_API.DTO
{
    public class CategoryDTO
    {
        [Required]
        [MaxLength(100, ErrorMessage = "must be less than 100")]
        public string Name { get; set; }

        public List<Product>? products { get; set; }
    }
}
