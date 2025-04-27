using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Identity_Roles_API.Data;
using Identity_Roles_API.Data.Models;
using Identity_Roles_API.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;

namespace Identity_Roles_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _iconfig;
        public AccountController(UserManager<AppUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IConfiguration iconfig)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _iconfig = iconfig;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser(RegsiterUserDTO user)
        {
            var normalizedrole= user.role.ToLower();
            if (normalizedrole == UserRoles.Admin || normalizedrole == UserRoles.User || normalizedrole == UserRoles.Manager)
            {
                bool roleExists = await _roleManager.RoleExistsAsync(normalizedrole);

                if (!roleExists)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        IsSuccess = false,
                        Message = $"The role '{user.role}' does not exists."
                    });
                }

                if (ModelState.IsValid)
                {
                    var newuser = _mapper.Map<AppUser>(user);
                    IdentityResult result = await _userManager.CreateAsync(newuser, user.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(newuser, normalizedrole); 
                        return Ok(new ApiResponse<string> { Message = "User registered", IsSuccess = true });
                    }
                    else
                    {
                        return BadRequest(result.Errors);
                    }
                }
            }
            else
            {
                return BadRequest(new ApiResponse<string> { IsSuccess = false, Message = "This role is not valid" });
            }

            return BadRequest(ModelState);
        }

        [HttpPost("AddRoleToUser")]
        public async Task<IActionResult> AddRoleToUser(string userEmail, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Role added successfully");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser(LoginDTO login)
        {
            if (ModelState.IsValid)
            {
                AppUser? user = await _userManager.FindByNameAsync(login.UserName);
                if (user == null)
                {
                    return NotFound(new ApiResponse<string> { IsSuccess = false, Message = "no user with this username" });
                }
                else
                {
                    if (await _userManager.CheckPasswordAsync(user, login.Password))
                    {
                      var claims=new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Name,user.UserName));
                        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        var roles = await _userManager.GetRolesAsync(user);
                        
                        if (roles != null && roles.Count > 0)
                        {
                            foreach (var role in roles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                        }
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_iconfig["JWT:SecretKey"]));
                        var sc = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            claims: claims,
                            issuer: _iconfig["JWT:Issuer"],
                            audience: _iconfig["JWT:Audience"],
                            expires: DateTime.Now.AddHours(5),
                            signingCredentials: sc
                            );
                        var _token = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };
                        return Ok(_token);
                    }
                    {
                        return Unauthorized();
                    }
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
