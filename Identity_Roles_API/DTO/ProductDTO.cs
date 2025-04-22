using Identity_Roles_API.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Identity_Roles_API.DTO
{
    public class ProductDTO
    {
       
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }

        [Range(1, 10000)]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }

    }
}
