using Catalog.BindingModel;
using Catalog.Data.Entities;
using Catalog.DataTransferObject;
using Catalog.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]  
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JWTConfig jWTConfig;

        public UserController(ILogger<UserController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IOptions<JWTConfig> options)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            jWTConfig = options.Value;
            _logger = logger;
        }
        
        [HttpPost("RegisterUser")]
        public async Task<object> RegisterUSer([FromBody] AddUpdateRegisterUserBindingModel model)
        {
            try
            {


                var user = new AppUser() {UserName = model.Email, FullName = model.FullName, Email = model.Email, DateCreated = DateTime.UtcNow, DateModified = DateTime.UtcNow };
                var results = await _userManager.CreateAsync(user, model.Password);
                if (results.Succeeded)
                {
                    return await Task.FromResult("User has been Registered");
                }
                return await Task.FromResult(string.Join(",", results.Errors.Select(x => x.Description).ToArray()));
            }catch(Exception ex)
            {
                return await Task.FromResult(ex.Message);
            }

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetAllUser")]
        public async Task<object> GetAllUser()
        {
            try
            {
                var users = _userManager.Users.Select(x=> new UserDTO(x.FullName,x.Email,x.UserName,x.DateCreated));
                return await Task.FromResult(users);
            }catch(Exception ex)
            {
                return await Task.FromResult(ex.Message);
            }
        }

        [HttpPost("login")]

        public async Task<object> Login([FromBody] LoginBindingModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var results = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                    if (results.Succeeded)
                    {
                        var appUser = await _userManager.FindByEmailAsync(model.Email);
                        var user = new UserDTO(appUser.FullName, appUser.Email, appUser.UserName, appUser.DateCreated);
                        user.Token = GenerateToken(appUser);
                        return await Task.FromResult(user);
                    }
                }
               
                return await Task.FromResult("invalid details");
               
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string GenerateToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jWTConfig.Key);
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.NameId,user.Id),
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email,user.Email),
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                }),
                Expires=DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256),
                Audience = jWTConfig.Audience,
                Issuer = jWTConfig.Issuer
            };
            var token = tokenHandler.CreateToken(tokenDescription);
            return tokenHandler.WriteToken(token);
        }
    }

   
}
