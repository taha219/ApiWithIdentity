using AutoMapper;
using Identity_Roles_API.Data;
using Identity_Roles_API.Data.Models;
using Identity_Roles_API.DTO;
using Identity_Roles_API.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NPOI.SS.Formula.Functions;

namespace Identity_Roles_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IBase<Product> _ibase;
        private readonly IBase<Category> _category;
        private readonly IStringLocalizer<ProductController> _localizer;
        private readonly IMapper _mapper;

        public ProductController(IBase<Product> ibase, IBase<Category> category,IStringLocalizer<ProductController> localizer, IMapper mapper)
        {
            _ibase = ibase;
            _category = category;
            _localizer = localizer;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task <IActionResult> GetAllProducts()
        {
          var products = await _ibase.GetAllAsync();
            
            return Ok(new ApiResponse<IEnumerable<Product>>
            {
                IsSuccess = true,
                Message = _localizer["getallproducts"],
                Data = products
            });
        }
        [HttpGet("GetById")]
        public async Task<IActionResult> GetProductById(int id) 
            {

              var product = await _ibase.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new ApiResponse<Product>
                    {
                        IsSuccess = true,
                        Message = "no product with this id",
                       
                    });
                }
            return Ok(new ApiResponse<Product> { IsSuccess = true, Message = "you got product", Data = product });
            }
        [HttpGet("GetByName")]
        public async Task<IActionResult> GetProductByName(string name)
        {
            var product = await _ibase.FindAsync(n => n.Name == name, new[] {"category"});

            if (product != null)
            {
                var message =product.category!=null? $"Product '{name}' exists and has a category."
            : $"Product '{name}' exists but has no category.";
                return Ok(new ApiResponse<Product> { IsSuccess = true, Message =message,Data= product });
            }
            return NotFound(new ApiResponse<Product> { IsSuccess = false, Message = $"{name}is not exists" });
        }
        [HttpPost]
        public async Task<IActionResult> InsertProduct([FromBody] ProductDTO productdto)
        {
            if (productdto == null)
            {
                return BadRequest(new ApiResponse<Product> { IsSuccess = false, Message = "product not added" });
            }
            var category = await _category.GetByIdAsync(productdto.CategoryId);
            if (category == null)
            {
                return BadRequest(new ApiResponse<Product> { IsSuccess = false, Message = "we do not have this category" });
            }
            // Manual Mapping 
            //var productEntity = new Product
            //{
            //    Name = productdto.Name,
            //    Price = productdto.Price,
            //    CategoryId = productdto.CategoryId
            //};

            //auto mapping
            var productEntity = _mapper.Map<Product>(productdto);  
            var newproduct = await _ibase.InsertItemAsync(productEntity);

            return CreatedAtAction(nameof(GetProductById), new { id = newproduct.Id },
                new ApiResponse<Product> { IsSuccess=true ,Message = "product added succefully",Data=newproduct});
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int id)

        {
            try
            {
                await _ibase.DeleteItemAsync(id);
                return Ok(new ApiResponse<Product> { IsSuccess = true, Message = "product deleted succefully" });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<Product> { IsSuccess = false, Message = ex.Message });
            }
        }
    }
}
