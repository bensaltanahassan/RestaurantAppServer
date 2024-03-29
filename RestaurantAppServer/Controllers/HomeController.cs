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
    }
}