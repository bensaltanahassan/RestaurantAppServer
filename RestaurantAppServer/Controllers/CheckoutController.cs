using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Models;

namespace RestaurantAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CheckoutController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Checkout(int id, [FromBody] OrderModel order)
        {
            try
            {
                var user = await _db.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { status = false, message = "User not found" });
                }
                Order newOrder = new()
                {   
                    Adress = order.Adress,
                    UserId = id,
                    PaymentStatus = order.PaymentStatus,
                    PaymentMethod = order.PaymentMethod,
                    TotalPrice = order.TotalPrice,
                    PhoneNumber = order.PhoneNumber,
                    OrderStatus = order.OrderStatus,
                };
                await _db.Orders.AddAsync(newOrder);
                await _db.SaveChangesAsync();
                _db.OrderItems
                    .Where(oi => oi.OrderId == null && oi.UserId == id)
                    .ToList()
                    .ForEach(oi => oi.OrderId = newOrder.Id);

                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Checkout with success" });

            }
            catch (Exception err)
            {
                return BadRequest(new { status = false, message = "Internal Server Error", error = err.Message });
            }
        }

    }
}
