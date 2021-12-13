using Catalog.BindingModel;
using Catalog.Data.Entities;
using Catalog.DataTransferObject;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public UserController(ILogger<UserController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
    }

   
}
