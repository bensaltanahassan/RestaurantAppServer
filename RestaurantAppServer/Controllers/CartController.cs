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
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CartController(AppDbContext db) => _db = db;


        [HttpGet()]
        public async Task<IActionResult> GetAllProductFromCart([FromBody] CartModel cm)
        {
            try
            {
                int userId = cm.userId;
                var cart = await _db.OrderItems
                    .Where(oi => oi.UserId == userId && oi.order == null)
                    .Include(oi => oi.product)
                    .ToListAsync();
                return Ok(new { status = true, cart });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] OrderItemModel orderItem)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == orderItem.ProductId);
                if (product == null)
                {
                    return NotFound(new { status = false, message = "Product not found" });
                }
                int userId = orderItem.UserId;
                var cart = await _db.OrderItems
                    .Where(oi => oi.UserId == userId && oi.order == null)
                    .Include(oi => oi.product)
                    .ToListAsync();
                var existingItem = cart.FirstOrDefault(oi => oi.ProductId == orderItem.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += orderItem.Quantity;
                    _db.OrderItems.Update(existingItem);
                }
                else
                {
                    OrderItem newOi = new()
                    {
                        Quantity = orderItem.Quantity,
                        ProductId = orderItem.ProductId,
                        UserId = userId,
                    };
                    _db.OrderItems.Add(newOi);
                }
                await _db.SaveChangesAsync();
                return StatusCode(201, new { status = true, message = "Add with success" });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFromCart(int id)
        {
            try
            {
                var oi = await _db.OrderItems.FindAsync(id);
                if (oi == null)
                {
                    return NotFound(new { status = false, message = $"Cart Item not found with id {id}" });
                }
                _db.OrderItems.Remove(oi);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Deleted successfuly" });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> IncDecQuantity(int id, [FromBody] OrderItemModel oim)
        {
            try
            {
                int quantity = oim.Quantity;
                int oiId = id;
                var oi = await _db.OrderItems.FindAsync(id);
                if (oi == null)
                {
                    return BadRequest(new { status = false, message = "Cart Item not found" });
                }
                // if  (cart.quantity===1 and quantity===-1) ==> remove it
                if (oi.Quantity == 1 && quantity == -1)
                {
                    _db.OrderItems.Remove(oi);
                }
                else
                {
                    // if the cart.quantity>1 the he could add or remove 1
                    oi.Quantity += quantity;
                }
                await _db.SaveChangesAsync();

                return Ok(new { status = false, message = "Success" });

            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }

        }


    }
}
