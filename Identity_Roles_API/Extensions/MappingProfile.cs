
using AutoMapper;
using Identity_Roles_API.Data.Models;
using Identity_Roles_API.DTO;

namespace Identity_Roles_API.Extensions
{
    public class MappingProfile :Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductDTO,Product>();
            CreateMap<CategoryDTO, Category>();
            CreateMap<RegsiterUserDTO,AppUser>();

            CreateMap<Product, ProductDTO>();
            CreateMap<Category, CategoryDTO>();
        }
    }
}
