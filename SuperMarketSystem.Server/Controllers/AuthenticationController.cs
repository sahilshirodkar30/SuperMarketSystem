using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SuperMarketSystem.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SuperMarketSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AuthenticationController(UserManager<ApplicationUser> userManager,
          RoleManager<IdentityRole> roleManager,
          IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error Message", Message = "Username already exists" });
            }

            ApplicationUser user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                { Status = "Error Message", Message = "User Created Failed" });

            }
            else
            {
                if(!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    if(!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                        await _userManager.AddToRoleAsync(user,"User");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                }
            }
            return Ok(new Response { Status = "Success", Message = "User Created Successfully" });


        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.
                CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),
                };
                foreach (var UserRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, UserRole));
                }


                var authSignInKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var toekn = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials
                    (authSignInKey, SecurityAlgorithms.HmacSha256)
                       );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(toekn)
                });
            }
            return Unauthorized();

        }


    }

}
