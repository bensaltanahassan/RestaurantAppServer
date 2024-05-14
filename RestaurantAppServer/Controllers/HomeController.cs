using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.helpers.enums;
using RestaurantAppServer.Models;

namespace RestaurantAppServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        [Route("mobile")]
        public async Task<IActionResult> GetHomeElement([FromQuery] int limitPerCategory = 10)
        {
            try
            {
                var topProducts = await _db.Products.Take(limitPerCategory)
                    .Where(p => p.IsAvailable)
                    .OrderByDescending(p => p.Discount)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.NameAn,
                        p.Description,
                        p.DescriptionAn,
                        p.Price,
                        p.Discount,
                        p.NbrOfSales,
                        p.IsAvailable,
                        p.CategoryId,
                        Category = new Category
                        {
                            Id = p.Category.Id,
                            Name = p.Category.Name,
                            NameAn = p.Category.NameAn
                        },
                        p.CreatedAt,
                        p.UpdatedAt,
                        ProductImages = p.ProductImages.Select(pi => new ProductImages
                        {
                            Id = pi.Id,
                            ImageId = pi.ImageId,
                            image = new Image
                            {
                                Id = pi.image.Id,
                                PublicId = pi.image.PublicId,
                                Url = pi.image.Url
                            }
                        }).ToList()
                    }).ToArrayAsync();

                var categories = await _db.Categories
                    .Include(c => c.image)
                    .Select(
                        c => new
                        {
                            Category = c,
                            Products = _db.Products
                                .OrderByDescending(p => p.NbrOfSales)
                                .OrderByDescending(p => p.Discount)
                                .Where(p => p.CategoryId == c.Id && p.IsAvailable)
                                .Take(limitPerCategory)
                                .Select(p => new
                                {
                                    p.Id,
                                    p.Name,
                                    p.NameAn,
                                    p.Description,
                                    p.DescriptionAn,
                                    p.Price,
                                    p.Discount,
                                    p.NbrOfSales,
                                    p.IsAvailable,
                                    p.CategoryId,
                                    Category = new Category
                                    {
                                        Id = p.Category.Id,
                                        Name = p.Category.Name,
                                        NameAn = p.Category.NameAn
                                    },
                                    p.CreatedAt,
                                    p.UpdatedAt,
                                    ProductImages = p.ProductImages.Select(pi => new ProductImages
                                    {
                                        Id = pi.Id,
                                        ImageId = pi.ImageId,
                                        image = new Image
                                        {
                                            Id = pi.image.Id,
                                            PublicId = pi.image.PublicId,
                                            Url = pi.image.Url
                                        }
                                    }).ToList()
                                }).ToList()
                        }
                    ).ToListAsync();


                return Ok(new { status = true, categories, topProducts });


            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = err.Message });
            }
        }

        [HttpGet]
        [Route("PromoWeb")]
        public async Task<IActionResult> GetHomePromo()
        {
            try
            {
                var productsWithDiscounts = await _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                        .ThenInclude(pi => pi.image)
                    .Where(p => p.Discount > 0)
                    .ToListAsync();

                var productsPerCategory = productsWithDiscounts
                    .GroupBy(p => p.CategoryId)
                    .Select(group => new
                    {
                        CategoryId = group.Key,
                        Product = group.OrderBy(p => Guid.NewGuid()).FirstOrDefault() 
                    })
                    .Select(p => new
                    {
                        Id = p.Product?.Id,
                        Name = p.Product?.Name,
                        NameAn = p.Product?.NameAn,
                        Description = p.Product?.Description,
                        DescriptionAn = p.Product?.DescriptionAn,
                        Price = p.Product?.Price,
                        Discount = p.Product?.Discount,
                        NbrOfSales = p.Product?.NbrOfSales,
                        IsAvailable = p.Product?.IsAvailable,
                        CategoryId = p.CategoryId,
                        Category = new
                        {
                            Id = p.Product?.Category?.Id,
                            Name = p.Product?.Category?.Name,
                            NameAn = p.Product?.Category?.NameAn
                        },
                        CreatedAt = p.Product?.CreatedAt,
                        UpdatedAt = p.Product?.UpdatedAt,
                        ProductImages = p.Product?.ProductImages.Select(pi => new
                        {
                            Id = pi.Id,
                            ImageId = pi.ImageId,
                            image = new
                            {
                                Id = pi.image.Id,
                                PublicId = pi.image.PublicId,
                                Url = pi.image.Url
                            }
                        }).ToList()
                    }).ToList();

                return Ok(new { status = true, productsPerCategory });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = err.Message });
            }
        }


        [HttpGet]
        [Route("RandomProductWeb")]
        public async Task<IActionResult> GetRandomProducts([FromQuery] int limit)
        {
            try
            {
                if (limit <= 0)
                    return BadRequest("Invalid count value");

                var randomProducts = await _db.Products
                    .OrderBy(p => Guid.NewGuid()) 
                    .Take(limit) 
                    .Include(p => p.Category) 
                    .Include(p => p.ProductImages)
                        .ThenInclude(pi => pi.image)
                    .ToListAsync();

                var formattedProducts = randomProducts.Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    NameAn = p.NameAn,
                    Description = p.Description,
                    DescriptionAn = p.DescriptionAn,
                    Price = p.Price,
                    Discount = p.Discount,
                    NbrOfSales = p.NbrOfSales,
                    IsAvailable = p.IsAvailable,
                    CategoryId = p.CategoryId,
                    Category = new
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name,
                        NameAn = p.Category.NameAn
                    },
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    ProductImages = p.ProductImages.Select(pi => new ProductImages
                    {
                        Id = pi.Id,
                        ImageId = pi.ImageId,
                        image = new Image
                        {
                            Id = pi.image.Id,
                            PublicId = pi.image.PublicId,
                            Url = pi.image.Url
                        }
                    }).ToList()
                }).ToList();

                return Ok(new { status = true, data = formattedProducts });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error" });
            }
        }

        [HttpGet]
        [Route("GetReview")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            var reviews = await _db.Reviews
                .Include(r => r.user)
                .Include(r => r.user.image)
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpPost]
        [Route("AddReview")]
        public async Task<IActionResult> CreateReview([FromForm] ReviewModel reviewModel)
        {
            try
            {
                var review = new Review
                {
                    UserId = reviewModel.UserId,
                    Comment = reviewModel.Comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Reviews.Add(review);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Review created successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message });
            }
        }
        [HttpGet("DashboardData")]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var totalUsers = await _db.Users.CountAsync();
                var totalOrders = await _db.Orders.CountAsync();
                var totalDeliveryMen = await _db.DeliveryMen.CountAsync();
                var totalReservations = await _db.Reservations.CountAsync();
                var reservations = await _db.Reservations.ToListAsync();
                var deliveriesPerDayOfWeek = reservations
                    .GroupBy(d => d.CreatedAt.DayOfWeek)
                    .Select(g => new { DayOfWeek = (int)g.Key, Count = g.Count() })
                    .OrderBy(g => g.DayOfWeek)
                    .ToList();
                var totalOrdersPending = await _db.Orders
                    .Where(x => x.OrderStatus == DeliveryStatus.Pending.ToString())
                    .CountAsync();
                var totalOrdersDelivered = await _db.Orders
                    .Where(x => x.OrderStatus == DeliveryStatus.Delivered.ToString())
                    .CountAsync();
                var totalOrdersShipping = await _db.Orders
                    .Where(x => x.OrderStatus == DeliveryStatus.Shipping.ToString())
                    .CountAsync();
                return Ok(new
                {
                    status = true,
                    meesage = "Data retrived successfully",
                    data = new
                    {
                        totalUsers,
                        totalOrders,
                        totalDeliveryMen,
                        totalReservations,
                        deliveriesPerDayOfWeek,
                        totalOrdersPending,
                        totalOrdersDelivered,
                        totalOrdersShipping
                    }
                });
                
            }catch(Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }

    }
}