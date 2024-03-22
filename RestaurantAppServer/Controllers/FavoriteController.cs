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
    public class FavoriteController : ControllerBase
    {
        private readonly AppDbContext _db;
        public FavoriteController(AppDbContext db)
        { 
            _db = db; 
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAllProductsInFavorites(int userId)
        {
            try
            {
                var favorites = await _db.Favorites
                    .Where(f => f.UserId == userId)
                    .Include(f => f.product)
                        .ThenInclude(p => p.Category)
                    .Include(f => f.product)
                        .ThenInclude(p => p.ProductImages)
                            .ThenInclude(pi => pi.image)
                    .Select(f => new
                    {
                        Product = new Product
                        {
                            Id = f.product.Id,
                            Name = f.product.Name,
                            NameAn = f.product.NameAn,
                            Description = f.product.Description,
                            DescriptionAn = f.product.DescriptionAn,
                            Price = f.product.Price,
                            Discount = f.product.Discount,
                            NbrOfSales = f.product.NbrOfSales,
                            IsAvailable = f.product.IsAvailable,
                            CategoryId = f.product.CategoryId,
                            Category = new Category
                            {
                                Id = f.product.Category.Id,
                                Name = f.product.Category.Name,
                                NameAn = f.product.Category.NameAn
                            },
                            CreatedAt = f.product.CreatedAt,
                            UpdatedAt = f.product.UpdatedAt,
                            ProductImages = f.product.ProductImages.Select(pi => new ProductImages
                            {
                                Id = pi.Id,
                                image = pi.image
                            }).ToList()
                        }
                    })
                    .ToListAsync();

                return Ok(new { status = true, products = favorites.Select(f => f.Product).ToList() });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = err.Message });
            }
        }



        [HttpDelete("{favoriteId}")]
        public async Task<IActionResult> DeleteFavorite(int favoriteId)
        {
            try
            {
                var favorite = await _db.Favorites.FindAsync(favoriteId);

                if (favorite == null)
                {
                    return BadRequest(new { status = false, message = "Favorite not found" });
                }

                _db.Favorites.Remove(favorite);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Favorite deleted successfully" });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = err.Message });
            }
        }


        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteAllProductsFromFavorites(int userId)
        {
            try
            {
               
                var favoritesToDelete = await _db.Favorites
                    .Where(f => f.UserId == userId)
                    .ToListAsync();

                if (favoritesToDelete.Count == 0)
                {
                    return BadRequest(new { status = false, message = "Favorites not found for the user" });
                }

                
                _db.Favorites.RemoveRange(favoritesToDelete);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "All products deleted from favorites successfully" });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = err.Message });
            }
        }


        [HttpPost]
        [Route("AddToFavorites")]
        public async Task<IActionResult> AddToFavorites([FromBody] FavoriteModel favoriteModel)
        {
            try
            {
                var user = await _db.Users.FindAsync(favoriteModel.UserId);
                var product = await _db.Products.FindAsync(favoriteModel.ProductId);

                if (user == null || product == null)
                {
                    return BadRequest(new { status = false, message = "User or product not found" });
                }

                var existingFavorite = await _db.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == favoriteModel.UserId && f.ProductId == favoriteModel.ProductId);

                if (existingFavorite != null)
                {
                    return BadRequest(new { status = false, message = "Product already in favorites" });
                }

                var favorite = new Favorite
                {
                    UserId = favoriteModel.UserId,
                    ProductId = favoriteModel.ProductId
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


    }
}
