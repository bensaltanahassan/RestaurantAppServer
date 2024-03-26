using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Models.auth;
using RestaurantAppServer.Models.auth.admin;
using RestaurantAppServer.Models.auth.user;
using RestaurantAppServer.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestaurantAppServer.Controllers.auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly JwtTokenService _jwtTokenService;
        public AdminAuthController(AppDbContext db, IConfiguration config,JwtTokenService jwtTokenService)
        {
            _db = db;
            _config = config;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginAdmin adminObj)
        {
            var admin = await _db.Admins.FirstOrDefaultAsync(u => u.Email == adminObj.Email);
            if (admin == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User doesn't exist! " });
            }
            if (adminObj.Password==admin.Password)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, admin.Email),
                    new Claim(ClaimTypes.Role, "admin"),
                    new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString())
                };
                var jwtToken = _jwtTokenService.GetToken(claims);
                var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                return Ok(new
                {
                    token = token,
                    expiration = jwtToken.ValidTo,
                    admin = new
                    {
                        Email = admin.Email,
                    }
                });
            }
            return StatusCode(StatusCodes.Status401Unauthorized, new Response { Status = "Error", Message = "Username or password are incorrect!" });
        }
        [HttpPost("resetPassword")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordAdmin resetPassword)
        {
            var admin = await _db.Admins.FirstOrDefaultAsync(u => u.Email == resetPassword.Email);
            if (admin == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Admin doesn't exist! " });
            }
            admin.Password = resetPassword.Password;
            _db.Admins.Update(admin);
            await _db.SaveChangesAsync();
            return Ok(new Response { Status = "Success", Message = "Password has been reset!" });
        }
    }
}
