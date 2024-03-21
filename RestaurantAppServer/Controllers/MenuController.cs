using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantAppServer.Interfaces;
using RestaurantAppServer.Services;


namespace RestaurantAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IImageService _imageService;

        public MenuController(AppDbContext db, IImageService imageService)
        {
            _db = db;
            _imageService = imageService;
        }

        [HttpGet]
        [Route("GetItems")]
        public async Task<ActionResult<IEnumerable<Product>>> GetItems()
        {
            var productsWithFirstImage = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                    .ThenInclude(pi => pi.image)
                .Select(p => new Product
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
                    Category = new Category 
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
                        image = pi.image 
                    }).ToList()
                })
                .ToListAsync();

            return Ok(productsWithFirstImage);
        }



        [HttpPost]
        [Route("CreateItems")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductModel pm, IFormFile file)
        {
            try
            {
                var result = await _imageService.AddImageAsync(file);
                if (result.Error != null)
                    return BadRequest(new { status = false, message = "Image upload failed" });

                ImageModel img = new()
                {
                    PublicId = result.PublicId,
                    Url = result.SecureUrl.AbsoluteUri
                };

                Image image = new()
                {
                    PublicId = img.PublicId,
                    Url = img.Url
                };

                await _db.Images.AddAsync(image);
                await _db.SaveChangesAsync();

                Product product = new()
                {
                    Name = pm.Name,
                    NameAn = pm.NameAn,
                    Description = pm.Description,
                    DescriptionAn = pm.DescriptionAn,
                    Price = pm.Price,
                    Discount = pm.Discount,
                    NbrOfSales = pm.NbrOfSales,
                    IsAvailable = pm.IsAvailable,
                    CategoryId = pm.CategoryId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ProductImages = new List<ProductImages>
            {
                new ProductImages
                {
                    ImageId = image.Id
                }
            }
                };

                await _db.Products.AddAsync(product);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Product created successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductModel pm)
        {
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                {
                    return BadRequest(new { status = false, message = "Product not found" });
                }

                product.Name = pm.Name;
                product.NameAn = pm.NameAn;
                product.Description = pm.Description;
                product.DescriptionAn = pm.DescriptionAn;
                product.Price = pm.Price;
                product.Discount = pm.Discount;
                product.NbrOfSales = pm.NbrOfSales;
                product.IsAvailable = pm.IsAvailable;
                product.CategoryId = pm.CategoryId;

                _db.Products.Update(product);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Product updated successfully" });
            }
            catch
            {
                return BadRequest(new { status = false, message = "Internal Server Error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                {
                    return BadRequest(new { status = false, message = "Product not found" });
                }

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Product deleted successfully" });
            }
            catch
            {
                return BadRequest(new { status = false, message = "Internal Server Error" });
            }
        }

        [HttpPost]
        [Route("AddToFavorites")]
        public async Task<IActionResult> AddToFavorites(int userId, int productId)
        {
            try
            {
                var user = await _db.Users.FindAsync(userId);
                var product = await _db.Products.FindAsync(productId);

                if (user == null || product == null)
                {
                    return BadRequest(new { status = false, message = "User or product not found" });
                }

                var existingFavorite = await _db.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                if (existingFavorite != null)
                {
                    return BadRequest(new { status = false, message = "Product already in favorites" });
                }

                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId
                };

                await _db.Favorites.AddAsync(favorite);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Product added to favorites successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message });
            }
        }


        [HttpPost]
        [Route("AddToCart")]
        public async Task<IActionResult> AddToCart(int userId, int productId, int quantity)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
                if (product == null)
                {
                    return BadRequest(new { status = false, message = "Product not found" });
                }

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return BadRequest(new { status = false, message = "User not found" });
                }

                var cartItem = await _db.OrderItems.FirstOrDefaultAsync(oi => oi.UserId == userId && oi.ProductId == productId && oi.OrderId == null);
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                    _db.OrderItems.Update(cartItem);
                }
                else
                {
                    var newCartItem = new OrderItem
                    {
                        Quantity = quantity,
                        ProductId = productId,
                        UserId = userId,
                    };
                    _db.OrderItems.Add(newCartItem);
                }

                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Product added to cart successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message });
            }
        }


    }
}
