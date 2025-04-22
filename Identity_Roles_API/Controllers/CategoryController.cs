using AutoMapper;
using Identity_Roles_API.Data;
using Identity_Roles_API.Data.Models;
using Identity_Roles_API.DTO;
using Identity_Roles_API.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity_Roles_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IBase<Category> _ibase;
        private readonly IMapper _mapper;
        public CategoryController(IBase<Category> ibase, IMapper mapper)
        {
            _ibase = ibase;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetCategory()
        {
            
            var categories=await _ibase.GetAllAsyncWith(["products"]);

            if (categories!=null&&categories.Any())
            {
                var categoryDTOs = _mapper.Map<IEnumerable<CategoryDTO>>(categories);
                return Ok(new ApiResponse<IEnumerable<Category>>
                {
                    IsSuccess = true,
                    Message = "you got all categories with its products",
                    Data = categories,
                });
            }
            return NotFound(new ApiResponse<string> { IsSuccess = false, Message="no categories "});
        }
    }
}
