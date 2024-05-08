using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.helpers.enums;
using RestaurantAppServer.Models;

namespace RestaurantAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DelivriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DelivriesController(AppDbContext db)
        {
            _db = db;
        }
        [HttpPost("RegisterDeliveryMan")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDeliveryMan([FromForm] DeliveryManModel dm)
        {
            try
            {
                var deliveryManExist = _db.DeliveryMen.FirstOrDefault(x => x.Email == dm.Email);
                if (deliveryManExist != null)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { status = false, message = "Delivery man already exists" });
                }
                var deliveryMan = new DeliveryMan
                {
                    Email = dm.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(dm.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                _db.DeliveryMen.Add(deliveryMan);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "DeliveryMan Created Successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }
        [HttpGet("GetAllDeliveryMan")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDeliveryMan([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                if (page <= 0 || limit <= 0)
                    return BadRequest(new { status = false, message = "Invalid page or limit value" });
                IQueryable<DeliveryMan> query = _db.DeliveryMen;
                int totalDeliveryMen = await query.CountAsync();
                int offset = (page - 1) * limit;
                var deliveryMen = await query.Skip(offset).Take(limit).ToListAsync();
                var deliveryMenResponse = new List<DeliveryMenResponse>();
                foreach (var deliveryMan in deliveryMen)
                {
                    var ordersOnShipping = await _db.Delivries.Where(x => x.deliveryManId == deliveryMan.Id).Where(x=> x.Status==DeliveryStatus.Shipping.ToString()).CountAsync();
                    var ordersDelivered = await _db.Delivries.Where(x => x.deliveryManId == deliveryMan.Id).Where(x => x.Status == DeliveryStatus.Delivered.ToString()).CountAsync();
                    deliveryMenResponse.Add(new DeliveryMenResponse
                    {
                        Id = deliveryMan.Id,
                        Email = deliveryMan.Email,
                        OrdersOnShipping = ordersOnShipping,
                        OrdersDelivered = ordersDelivered,
                    });
                }
                return Ok(new { status = true, totalDeliveryMen, currentPage = page, data = deliveryMenResponse });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }
        [HttpGet("GetDeliveryManById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDeliveryManById(int id)
        {
            try
            {
                var deliveryManExist = await _db.DeliveryMen.FindAsync(id);
                if (deliveryManExist == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { status = false, message = "Delivery doesn't exist" });
                }
                var ordersOnShipping = await _db.Delivries.Where(x=>x.deliveryManId==id).Where(x => x.Status == DeliveryStatus.Shipping.ToString()).Include(x=>x.order.TotalPrice).ToListAsync();
                var ordersDelivered = await _db.Delivries.Where(x => x.deliveryManId == id).Where(x => x.Status == DeliveryStatus.Delivered.ToString()).Include(x => x.order.TotalPrice).ToListAsync();
                return Ok(new { status = true,message="Data retrived successfully", data = new { ordersOnShipping, ordersDelivered } });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }
        [HttpDelete("DeleteDeliveryMan/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDeliveryMan(int id)
        {
            try
            {
                var deliveryManExist = await _db.DeliveryMen.FindAsync(id);
                if (deliveryManExist == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { status = false, message = "Delivery doesn't exist" });
                }
                _db.DeliveryMen.Remove(deliveryManExist);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "DeliveryMan Deleted Successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }


    }
}
