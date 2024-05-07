using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;

namespace RestaurantAppServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public OrdersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromQuery] int? orderId, [FromQuery] string? OrderStatus, [FromQuery] int page = 1, [FromQuery] int limit = 30)
        {
            try
            {
                IQueryable<Order> query = _db.Orders;
                if (orderId != null)
                {
                    query = query.Where(o => o.Id == orderId);
                }
                if (OrderStatus != null)
                {
                    query = query.Where(o => o.OrderStatus == OrderStatus);
                }
                int totalItems = await query.CountAsync();
                int offset = (page - 1) * limit;
                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip(offset)
                    .Take(limit)
                    .Include(o => o.user)
                    .Select(o => new
                    {
                        o.Id,
                        o.TotalPrice,
                        o.Adress,
                        o.UserId,
                        User = new
                        {
                            o.user.FullName,
                            o.user.Email,
                        },
                        o.PhoneNumber,
                        o.PaymentMethod,
                        o.PaymentStatus,
                        o.OrderStatus,
                        o.CreatedAt,
                        o.UpdatedAt,
                    }).ToListAsync();
                return Ok(new { status = true, orders, totalItems });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err.Message });
            }
        }




        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var order = await _db.Orders
                    .Where(o => o.Id == orderId)
                    .Include(o => o.user)
                    .Include(o => o.orderItems)
                        .ThenInclude(oi => oi.product)
                            .ThenInclude(p => p.ProductImages)
                                .ThenInclude(pi => pi.image)
                    .Select(o => new
                    {
                        o.Id,
                        o.UserId,
                        User = new
                        {
                            o.user.FullName,
                            o.user.Email,
                            o.user.Phone,
                            o.user.Address,
                        },
                        o.TotalPrice,
                        o.Adress,
                        o.PhoneNumber,
                        o.PaymentMethod,
                        o.PaymentStatus,
                        o.OrderStatus,
                        o.CreatedAt,
                        o.UpdatedAt,
                        OrderItems = o.orderItems.Select(oi => new
                        {
                            oi.Id,
                            oi.Quantity,
                            oi.OrderId,
                            Product = new
                            {
                                oi.product.Id,
                                oi.product.Name,
                                oi.product.NameAn,
                                oi.product.Description,
                                oi.product.DescriptionAn,
                                oi.product.Price,
                                oi.product.Discount,
                                oi.product.NbrOfSales,
                                oi.product.IsAvailable,
                                oi.product.CategoryId,
                                Category = new
                                {
                                    oi.product.Category.Id,
                                    oi.product.Category.Name,
                                    oi.product.Category.NameAn
                                },
                                oi.product.CreatedAt,
                                oi.product.UpdatedAt,
                                ProductImages = oi.product.ProductImages.Select(pi => new
                                {
                                    pi.Id,
                                    pi.image,
                                    pi.isMain
                                }).ToList()
                            },
                            oi.CreatedAt,
                            oi.UpdatedAt
                        }).ToList()
                    }).FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(new { status = false, message = "Order not found" });
                }

                return Ok(new { status = true, order });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err.Message });
            }

        }


        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetAllOrdersByUser([FromRoute] int userId, [FromQuery] int page = 1, [FromQuery] int limit = 30)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound(new { status = false, message = "User not found" });
                }
                IQueryable<Order> query = _db.Orders.Where(o => o.UserId == userId);
                int totalItems = await query.CountAsync();
                int offset = (page - 1) * limit;
                var orders = await query
                    .Skip(offset)
                    .Take(limit)
                    .Select(o => new
                    {
                        o.Id,
                        o.UserId,
                        o.TotalPrice,
                        o.Adress,
                        o.PhoneNumber,
                        o.PaymentMethod,
                        o.PaymentStatus,
                        o.OrderStatus,
                        o.CreatedAt,
                        o.UpdatedAt,
                    }).ToListAsync();
                return Ok(new { status = true, orders });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = err.Message });
            }
        }
    }
}