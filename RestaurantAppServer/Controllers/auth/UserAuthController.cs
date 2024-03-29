using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Models.auth.user;
using RestaurantAppServer.Service.Services;
using RestaurantAppServer.Models.auth;
using RestaurantAppServer.Service.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using RestaurantAppServer.Services;

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
            try
            {
                var userExists = await _db.Users.FirstOrDefaultAsync(u => u.Email == userObj.Email);
                if (userExists != null)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new Response { Status = false, Message = "User already exists!" });
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
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
                var jwtToken = _jwtTokenService.GetToken(claims);
                var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                user.EmailVerificationToken = token;

                var result = await _db.Users.AddAsync(user);

                if (result.State != EntityState.Added)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = false, Message = "Internal Server Error" });
                }
                await _db.SaveChangesAsync();

                var confirmationLink = Url.Action("ConfirmEmail", "UserAuth", new { token = token, email = user.Email }, Request.Scheme);
                var message = new Message(new string[] { user.Email }, "Email Confirmation", $"<h1>Welcome to Restaurant App</h1><p>Please confirm your email by <a href='{confirmationLink}'>clicking here</a></p>");
                _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status201Created, new Response { Status = true, Message = $"User created and email sent to {user.Email} successfully!" });

            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = e.Message });
            }
        }
        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new Response { Status = false, Message = "User doesn't exist! " });
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
                        return StatusCode(StatusCodes.Status200OK, new Response { Status = true, Message = "Email confirmed successfully" });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = false, Message = "Email failed to be confirmed! " });
                    }
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = e.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUser userObj)
        {
            try
            {
                var user = await _db.Users.Where(u => u.Email == userObj.Email)
                            .Include(u => u.image).FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound(new Response { Status = false, Message = "User doesn't exist!" });
                }
                
                if (!BCrypt.Net.BCrypt.Verify(userObj.Password, user.Password))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new Response { Status = "Error", Message = "Email or password are incorrect!" });
                }
                if (!user.IsVerified)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new Response { Status = "Error", Message = "Email is not verified!" });
                }
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
                    status = true,
                    data = new
                    {
                        token,
                        expiration = jwtToken.ValidTo,
                        user = new
                        {
                            user.Id,
                            user.FullName,
                            user.Email,
                            user.Phone,
                            user.Address,
                            user.ImageId,
                            user.image
                        }
                    }
                });

            } catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = e.Message });
            }
        }

        [HttpPost("forgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword fp)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == fp.Email);
                if (user != null)
                {
                    var confirmationCode = Guid.NewGuid().ToString();
                    user.ResetCode = confirmationCode;
                    user.ResetCodeExpiry = DateTime.UtcNow.AddMinutes(10);
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync();
                    Message message = new(new string[] { user.Email! }, "Confirmation code email", $"Hi , {user.FullName} this is your confirmation code for reseting your password : '{confirmationCode}'");

                    _emailService.SendEmail(message);
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = true, Message = $"Reset password code sent successfully to {user.Email}" });
                }

                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = false, Message = "User doesn't exist! " });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = e.Message });
            }
        }
        [HttpPost("resetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordUser resetPassword)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == resetPassword.Email);
                if (user == null)
                {
                    return NotFound(new Response { Status = false, Message = "User doesn't exist! " });
                }
                if (user.ResetCode != resetPassword.ResetCode || user.ResetCodeExpiry < DateTime.UtcNow)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = false, Message = "Reset code is invalid or expired!" });

                }
                user.Password = BCrypt.Net.BCrypt.HashPassword(resetPassword.Password);
                user.ResetCode = null;
                user.ResetCodeExpiry = null;
                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                return Ok(new Response { Status = true, Message = "Password has been reset !" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = e.Message });
            }
        }


    }
}
