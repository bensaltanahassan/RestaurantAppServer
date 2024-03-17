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

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] OrderModel order) {
            try {
                Order newOrder = new()
                {
                    Adress = order.Adress,
                    UserId = order.UserId,
                    PaymentStatus = order.PaymentStatus,
                    PaymentMethod = order.PaymentMethod,
                    TotalPrice = order.TotalPrice,
                    PhoneNumber = order.PhoneNumber,
                    OrderStatus = order.OrderStatus,
                };
                await _db.Orders.AddAsync(newOrder);
                _db.OrderItems
                    .Where(oi => oi.OrderId == null && oi.UserId == order.UserId)
                    .ToList()
                    .ForEach(oi => oi.OrderId = newOrder.Id);

                await _db.SaveChangesAsync();
                return Ok(new {status=true,message="Chekout with success"});

            }
            catch {
                return BadRequest(new { status = false, message = "Internal Server Error" });
            }
        }
    }
}
