﻿using Microsoft.AspNetCore.Authorization;
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
        [HttpGet("GetDeliveriesByDeliveryManId/{deliveryManId}")]
        public async Task<IActionResult> GetDeliveriesByDeliveryManId([FromRoute] int deliveryManId, [FromQuery] int? deliveryId, [FromQuery] string? deliveryStatus, [FromQuery] int page = 1, [FromQuery] int limit = 30)
        {
            try
            {
                if (page <= 0 || limit <= 0)
                    return BadRequest(new { status = false, message = "Invalid page or limit value" });
                IQueryable<Delivery> query = _db.Delivries
                    .Where(x => x.deliveryManId == deliveryManId)
                    .Include(x => x.order)
                        .ThenInclude(x => x.user)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new Delivery
                    {
                        Id = x.Id,
                        Status = x.Status,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        deliveryManId = x.deliveryManId,
                        deliveryMan = x.deliveryMan,
                        orderId = x.orderId,
                        order = new Order
                        {
                            Id = x.order.Id,
                            OrderStatus = x.order.OrderStatus,
                            CreatedAt = x.order.CreatedAt,
                            UpdatedAt = x.order.UpdatedAt,
                            UserId = x.order.UserId,
                            Adress = x.order.Adress,
                            PaymentStatus = x.order.PaymentStatus,
                            user = new User
                            {
                                Id = x.order.user.Id,
                                FullName = x.order.user.FullName,
                                Email = x.order.user.Email,
                                Phone = x.order.user.Phone,
                                Address = x.order.user.Address,
                            },
                            TotalPrice = x.order.TotalPrice,
                            PhoneNumber = x.order.PhoneNumber,
                            PaymentMethod = x.order.PaymentMethod,
                        }
                    });
                if (deliveryStatus != null)
                {
                    query = query.Where(x => x.Status == deliveryStatus);
                }
                if (deliveryId != null)
                {
                    query = query.Where(x => x.Id == deliveryId);
                }
                int totalDeliveries = await query.CountAsync();
                int offset = (page - 1) * limit;
                var deliveries = await query.Skip(offset).Take(limit).ToListAsync();
                return Ok(new { status = true, totalDeliveries, currentPage = page, data = deliveries });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }


        [HttpPost("RegisterDeliveryMan")]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateDeliveryMan([FromBody] DeliveryManModel dm)
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
                    FullName = dm.FullName,
                    PhoneNumber = dm.PhoneNumber,
                    Password = BCrypt.Net.BCrypt.HashPassword(dm.Password),
                    Status = DeliveryManStatus.Available.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                _db.DeliveryMen.Add(deliveryMan);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Delivery Man Created Successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }
        [HttpGet("GetAllDeliveryMan")]
        // [Authorize(Roles = "admin")]
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
                    var ordersOnShipping = await _db.Delivries.Where(x => x.deliveryManId == deliveryMan.Id).Where(x => x.Status == DeliveryStatus.Shipping.ToString()).CountAsync();
                    var ordersDelivered = await _db.Delivries.Where(x => x.deliveryManId == deliveryMan.Id).Where(x => x.Status == DeliveryStatus.Delivered.ToString()).CountAsync();
                    deliveryMenResponse.Add(new DeliveryMenResponse
                    {
                        Id = deliveryMan.Id,
                        Email = deliveryMan.Email,
                        FullName = deliveryMan.FullName,
                        PhoneNumber = deliveryMan.PhoneNumber,
                        Status = deliveryMan.Status,
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
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetDeliveryManById(int id)
        {
            try
            {
                var deliveryManExist = await _db.DeliveryMen.FindAsync(id);
                if (deliveryManExist == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { status = false, message = "Delivery doesn't exist" });
                }
                var ordersOnShipping = await _db.Delivries.Where(x => x.deliveryManId == id).Where(x => x.Status == DeliveryStatus.Shipping.ToString()).Include(x => x.order).ToListAsync();
                var ordersDelivered = await _db.Delivries.Where(x => x.deliveryManId == id).Where(x => x.Status == DeliveryStatus.Delivered.ToString()).Include(x => x.order).ToListAsync();
                return Ok(new { status = true, message = "Data retrived successfully", data = new { ordersOnShipping, ordersDelivered } });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }
        [HttpDelete("DeleteDeliveryMan/{id}")]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteDeliveryMan(int id)
        {
            try
            {
                var deliveryMan = await _db.DeliveryMen.FindAsync(id);
                if (deliveryMan == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { status = false, message = "Delivery doesn't exist" });
                }
                deliveryMan.Status = DeliveryManStatus.OffService.ToString();
                _db.DeliveryMen.Update(deliveryMan);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "DeliveryMan Deleted Successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }
        [HttpPut("UpdateDeliveryStatus/{orderId}")]
        // [Authorize(Roles ="delivery")]
        public async Task<IActionResult> UpdateDeliveryStatus(int orderId, [FromQuery] string status)
        {
            try
            {
                var delivery = await _db.Delivries.FirstOrDefaultAsync(x => x.orderId == orderId);
                if (delivery == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { status = false, message = "Delivery doesn't exist" });
                }
                var order = await _db.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { status = false, message = "Order doesn't exist" });
                }
                delivery.Status = status switch
                {
                    "delivered" => DeliveryStatus.Delivered.ToString(),
                    _ => DeliveryStatus.Shipping.ToString(),
                };
                delivery.UpdatedAt = DateTime.UtcNow;
                _db.Delivries.Update(delivery);
                order.OrderStatus = status switch
                {
                    "delivered" => DeliveryStatus.Delivered.ToString(),
                    _ => DeliveryStatus.Shipping.ToString(),
                };
                order.UpdatedAt = DateTime.UtcNow;
                _db.Orders.Update(order);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Status Updated Successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }

        [HttpPut("UpdateDeliveryManStatus/{id}")]
        // [Authorize(Roles = "delivery")]
        public async Task<IActionResult> UpdateDeliveryManStatus(int id, [FromQuery] string status)
        {
            try
            {
                var deliveryMan = await _db.DeliveryMen.FindAsync(id);
                if (deliveryMan == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { status = false, message = "Delivery doesn't exist" });
                }
                deliveryMan.Status = status switch
                {
                    "vacation" => DeliveryManStatus.Vacation.ToString(),
                    _ => DeliveryManStatus.Available.ToString(),
                };
                _db.DeliveryMen.Update(deliveryMan);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Status Updated Successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }


    }
}
