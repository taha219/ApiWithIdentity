using AutoMapper;
using Identity_Roles_API.Data;
using Identity_Roles_API.Data.Models;
using Identity_Roles_API.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity_Roles_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<AppUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser(RegsiterUserDTO user) 
        {
            if (Enum.TryParse<UserRole>(user.role, true, out var ParsedRole))
            {
                bool roleExists = await _roleManager.RoleExistsAsync(ParsedRole.ToString());

                if (!roleExists)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        IsSuccess = false,
                        Message = $"The role '{ParsedRole}' does not exist in database."
                    });
                }

                if (ModelState.IsValid)
                {
                    var newuser = _mapper.Map<AppUser>(user);
                    IdentityResult result = await _userManager.CreateAsync(newuser, user.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(newuser, ParsedRole.ToString());
                        return Ok(new ApiResponse<string> { Message = "user registered", IsSuccess = true});
                    }
                    else
                    {
                        return BadRequest(result.Errors);
                    }
                }
            }
            else
            {
                return BadRequest(new ApiResponse<string> { IsSuccess = false, Message = "this role is not found" });
            }
                return BadRequest(ModelState);
        }


    }
}
