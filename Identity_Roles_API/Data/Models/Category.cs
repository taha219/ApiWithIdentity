using System.ComponentModel.DataAnnotations;

namespace Identity_Roles_API.Data.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100,ErrorMessage ="must be less than 100")]
        public string Name { get; set; }

        public List<Product>? products { get; set; }
    }
}
