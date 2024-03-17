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
        public CartController(AppDbContext db)
        {
            _db = db;
        }

        /**
         * @description     Get all product
         * @router          /:id
         * @method          GET
         * @access          private(only logged in user)
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllProductFromCartController(int id)
        {
            try
            {
                int userId = id;
                var cart = await _db.OrderItems
                    .Where(oi => oi.UserId == userId && oi.order == null)
                    .Include(oi => oi.product)
                    .ToListAsync();
                return Ok(new {status=true,cart});
            }
            catch (Exception e)
            {
                return BadRequest(new { status = false, message = e.Message });
            }
        }

        /**
         * @description     Add to Cart
         * @router          /cart
         * @method          POST
         * @access          private(only logged in user)
         */
        [HttpPost]
        public async Task<IActionResult> AddToCartController([FromBody] OrderItemModel orderItem)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == orderItem.ProductId);
                if (product == null)
                {
                    return BadRequest(new {status=false,message= "Product not found" });
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
                    OrderItem newOi = new() {
                        Quantity = orderItem.Quantity,
                        ProductId = orderItem.ProductId,
                        UserId = userId,
                    };
                    _db.OrderItems.Add(newOi);
                }
                await _db.SaveChangesAsync();
                return Ok(new {status=true,message="Add with success"});
            }
            catch (Exception e)
            {
                return BadRequest(new { status = false, message = e.Message });
            }
        }

        /**
         * @description     DELETE from Cart
         * @router          /:id
         * @method          DELETE
         * @access          private(only logged in user)
         */

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFromCartController(int id)
        {
            try {
                int oiId = id;
                var oi = await _db.Orders.FirstOrDefaultAsync(o => o.Id == oiId);
                if (oi==null)
                {
                    return BadRequest(new {status=false,message= "Cart Item not found" });
                }
                _db.Orders.Remove(oi);
                await _db.SaveChangesAsync();
                return Ok(new {status=true,message= "Deleted successfuly" });
            }
            catch(Exception e)
            {
                return BadRequest(new { status = false, message = e.Message });
            }
        }

        /**
         * @description     Increase/Deacrease quanitity
         * @router          /:id
         * @method          PUT
         * @access          private(only logged in user)
         */
        [HttpPut("{id}")]
        public async Task<IActionResult> IncDecQuantityController(int id, [FromBody] int quantity){
            try {
                int oiId = id;
                var oi = await _db.OrderItems.SingleOrDefaultAsync(x=> x.Id == oiId);
                if (oi==null)
                {
                    return BadRequest(new { status = false, message="Cart Item not found" }) ;
                }
                // if  (cart.quantity===1 and quantity===-1) ==> remove it
                if (oi.Quantity==1 && quantity==-1)
                {
                    _db.OrderItems.Remove(oi);
                }
                // if the cart.quantity>1 the he could add or remove 1
                oi.Quantity += quantity;
                await _db.SaveChangesAsync();

                return Ok(new { status = false,message="Success" }) ;

            }
            catch (Exception e)
            {
                return BadRequest(new {status=false,message=e.Message});
            }

        }


    }
}
