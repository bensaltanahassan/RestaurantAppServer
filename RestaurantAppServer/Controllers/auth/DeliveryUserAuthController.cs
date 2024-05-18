using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Models.auth;
using RestaurantAppServer.Models.auth.admin;
using RestaurantAppServer.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.NetworkInformation;
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
            try
            {
                var deliveryMan = await _db.DeliveryMen.FirstOrDefaultAsync(u => u.Email == deliveryManObj.Email);
                if (deliveryMan == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new Response { Status = false, Message = "User doesn't exist! " });
                }
                if (BCrypt.Net.BCrypt.Verify(deliveryManObj.Password, deliveryMan.Password))
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
                        data = new
                        {
                            token,
                            expiration = jwtToken.ValidTo,
                            DeliveryMan = new
                            {
                                id = deliveryMan.Id,
                                email = deliveryMan.Email,
                                fullName = deliveryMan.FullName,
                                phoneNumber = deliveryMan.PhoneNumber,
                            }
                        },
                        status = true,
                    });
                }
                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Status = false, Message = "Username or password are incorrect!" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = e.Message });
            }
        }

        [HttpPost("resetPassword")]
        // [Authorize(Roles = "delivery")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDeliveryUser resetPassword)
        {
            try
            {
                var deliveryMan = await _db.DeliveryMen.FirstOrDefaultAsync(u => u.Email == resetPassword.Email);
                if (deliveryMan == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new Response { Status = false, Message = "Delivery User doesn't exist! " });
                }
                deliveryMan.Password = resetPassword.Password;
                _db.DeliveryMen.Update(deliveryMan);
                await _db.SaveChangesAsync();
                return Ok(new Response { Status = true, Message = "Password has been reset!" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = e.Message });
            }
        }
    }
}
