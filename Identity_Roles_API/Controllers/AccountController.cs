﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Identity_Roles_API.Data;
using Identity_Roles_API.Data.Models;
using Identity_Roles_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
                    //check if mail exists before
                    var existingUser = await _userManager.FindByEmailAsync(user.Email);
                    if (existingUser != null)
                    {
                        return BadRequest(new ApiResponse<string>
                        {
                            IsSuccess = false,
                            Message = "Email is already registered."
                        });
                    }
                    var newuser = _mapper.Map<AppUser>(user);
                    
                    //save picture to database
                    if (user.ProfilePicture != null && user.ProfilePicture.Length > 0)
                    {

                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(user.ProfilePicture.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            return BadRequest(new ApiResponse<string>
                            {
                                IsSuccess = false,
                                Message = "Only image files are allowed!"
                            });
                        }

                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfilePictures");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(user.ProfilePicture.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await user.ProfilePicture.CopyToAsync(fileStream);
                        }

                        newuser.ProfilePicture = "/ProfilePictures/" + uniqueFileName;
                    }
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
        [Authorize(Roles ="admin")]
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
                    user = await _userManager.FindByEmailAsync(login.UserName);

                    if (user == null)
                    {
                        return NotFound(new ApiResponse<string> { IsSuccess = false, Message = "No user with this username or email" });
                    }
                }

                if (await _userManager.CheckPasswordAsync(user, login.Password))
                {
                    var claims = new List<Claim>
                    {
                         new Claim(ClaimTypes.Name, user.UserName),
                         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };

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
                else
                {
                    return Unauthorized(new ApiResponse<string> { IsSuccess = false, Message = "Invalid password" });
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        [HttpGet]
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> GetAllUsersWithRoles()
        {
            var users = _userManager.Users.ToList();
            var usersWithRoles = new List<UsersWithRolesDto>();
            if (users != null)
            {
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    var userDto = _mapper.Map<UsersWithRolesDto>(user);
                    userDto.Roles = roles.ToList();

                    usersWithRoles.Add(userDto);
                }
                return Ok(usersWithRoles);
            }
            else 
            {
                return NotFound(new ApiResponse<string> { IsSuccess = false, Message = "no users fonud" });
            }
        }

    }
}
