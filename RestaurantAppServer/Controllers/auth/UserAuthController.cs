using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Models.auth.user;
using RestaurantAppServer.Service.Services;
using RestaurantAppServer.Models.auth;
using RestaurantAppServer.Service.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantAppServer.Controllers.auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        public UserAuthController(AppDbContext db, IEmailService emailService, IConfiguration config)
        {
            _db = db;
            _emailService = emailService;
            _config = config;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser userObj)
        {
            var userExists = await _db.Users.FirstOrDefaultAsync(u => u.Email == userObj.Email);
            if (userExists != null) { 
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Status = "Error", Message = "User already exists!" });
            }
            User user = new()
            {
                FullName = userObj.FullName,
                Email = userObj.Email,
                Phone = userObj.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(userObj.Password),
                Address = userObj.Adress,
                IsVerified = false
            };
            var result = await _db.Users.AddAsync(user);
            if(result.State != EntityState.Added)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Internal Server Error" });
            }
            await _db.SaveChangesAsync();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var token = GetToken(claims);
            var confirmationLink = Url.Action("ConfirmEmail", "UserAuth", new { token = token,email=user.Email }, Request.Scheme);
            var message = new Message(new string[] { user.Email }, "Email Confirmation", $"<h1>Welcome to Restaurant App</h1><p>Please confirm your email by <a href='{confirmationLink}'>clicking here</a></p>");
            _emailService.SendEmail(message);
            return StatusCode(StatusCodes.Status201Created, new Response { Status = "Success", Message = $"User created and email sent to {user.Email} successfully!" });

        }
        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            var message = new Message(new string[] { "a.hasna9422@uca.ac.ma" }, "Test", "<h1>Testing ....</h1>");
            _emailService.SendEmail(message);
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Email sent successfully" });

        }
        private JwtSecurityToken GetToken(List<Claim> claims)
        {
            var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SecretKey"]));
            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(1),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }
    }
}
