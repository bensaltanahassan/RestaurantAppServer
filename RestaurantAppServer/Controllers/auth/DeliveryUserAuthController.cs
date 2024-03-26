using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Models.auth;
using RestaurantAppServer.Models.auth.admin;
using RestaurantAppServer.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestaurantAppServer.Controllers.auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryUserAuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly JwtTokenService _jwtTokenService;
        public DeliveryUserAuthController(AppDbContext db, IConfiguration config, JwtTokenService jwtTokenService)
        {
            _db = db;
            _config = config;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginAdmin deliveryManObj)
        {
            var deliveryMan = await _db.DeliveryMen.FirstOrDefaultAsync(u => u.Email == deliveryManObj.Email);
            if (deliveryMan == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User doesn't exist! " });
            }
            if (deliveryManObj.Password == deliveryMan.Password)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, deliveryMan.Email),
                    new Claim(ClaimTypes.Role, "delivery"),
                    new Claim(ClaimTypes.NameIdentifier, deliveryMan.Id.ToString())
                };
                var jwtToken = _jwtTokenService.GetToken(claims);
                var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                return Ok(new
                {
                    token = token,
                    expiration = jwtToken.ValidTo,
                    admin = new
                    {
                        Email = deliveryMan.Email,
                    }
                });
            }
            return StatusCode(StatusCodes.Status401Unauthorized, new Response { Status = "Error", Message = "Username or password are incorrect!" });
        }
        [HttpPost("resetPassword")]
        [Authorize(Roles = "delivery")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDeliveryUser resetPassword)
        {
            var deliveryMan = await _db.DeliveryMen.FirstOrDefaultAsync(u => u.Email == resetPassword.Email);
            if (deliveryMan == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Delivery User doesn't exist! " });
            }
            deliveryMan.Password = resetPassword.Password;
            _db.DeliveryMen.Update(deliveryMan);
            await _db.SaveChangesAsync();
            return Ok(new Response { Status = "Success", Message = "Password has been reset!" });
        }
    }
}
