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
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using RestaurantAppServer.Utils;

namespace RestaurantAppServer.Controllers.auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly JwtTokenService _jwtTokenService;
        public UserAuthController(AppDbContext db, IEmailService emailService, IConfiguration config, JwtTokenService jwtTokenService)
        {
            _db = db;
            _emailService = emailService;
            _config = config;
            _jwtTokenService = jwtTokenService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser userObj)
        {
            var userExists = await _db.Users.FirstOrDefaultAsync(u => u.Email == userObj.Email);
            if (userExists != null)
            {
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
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var jwtToken = GetToken(claims);
            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            user.EmailVerificationToken = token;

            var result = await _db.Users.AddAsync(user);

            if (result.State != EntityState.Added)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Internal Server Error" });
            }
            await _db.SaveChangesAsync();

            var confirmationLink = Url.Action("ConfirmEmail", "UserAuth", new { token = token, email = user.Email }, Request.Scheme);
            var message = new Message(new string[] { user.Email }, "Email Confirmation", $"<h1>Welcome to Restaurant App</h1><p>Please confirm your email by <a href='{confirmationLink}'>clicking here</a></p>");
            _emailService.SendEmail(message);
            return StatusCode(StatusCodes.Status201Created, new Response { Status = "Success", Message = $"User created and email sent to {user.Email} successfully!" });

        }
        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User doesn't exist! " });
            }
            else
            {
                if (user.EmailVerificationToken == token)
                {
                    user.IsVerified = true;
                    user.EmailVerificationToken = null;
                    user.UpdatedAt = DateTime.UtcNow;
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Email confirmed successfully" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Email failed to be confirmed! " });
                }
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUser userObj)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userObj.Email);
            if (user == null)
            {
                return NotFound(new Response { Status = "Error", Message = "User doesn't exist!" });
            }
            if (BCrypt.Net.BCrypt.Verify(userObj.Password, user.Password))
            {
                var claims = new List<Claim>
                {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "user"),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
                var jwtToken = _jwtTokenService.GetToken(claims);
                var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                return Ok(new
                {
                    status = "Success",
                    token = token,
                    expiration = jwtToken.ValidTo,
                    user = new
                    {
                        user.FullName,
                        user.Email,
                        user.Phone,
                        user.Address
                    }
                });
            }
            return StatusCode(StatusCodes.Status401Unauthorized, new Response { Status = "Error", Message = "Email or password are incorrect!" });
        }

        [HttpPost("forgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([Required] string Email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (user != null)
            {
                var confirmationCode = Guid.NewGuid().ToString();
                user.ResetCode = confirmationCode;
                user.ResetCodeExpiry = DateTime.UtcNow.AddMinutes(10);
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
                Message message = new Message(new string[] { user.Email! }, "Confirmation code email", $"Hi , {user.FullName} this is your confirmation code for reseting your password : '{confirmationCode}'");

                _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = $"Reset password code sent successfully to {user.Email}" });
            }

            return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User doesn't exist! " });

        }
        [HttpPost("resetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordUser resetPassword)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == resetPassword.Email);
            if (user == null)
            {
                return NotFound(new Response { Status = "Error", Message = "User doesn't exist! " });
            }
            if (user.ResetCode != resetPassword.ResetCode || user.ResetCodeExpiry < DateTime.UtcNow)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Reset code is invalid or expired!" });

            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(resetPassword.Password);
            user.ResetCode = null;
            user.ResetCodeExpiry = null;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return Ok(new Response { Status = "Success", Message = "Password has been reset !" });
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
