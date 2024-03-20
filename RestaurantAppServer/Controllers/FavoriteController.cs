using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllProductsInFavorites(int userId)
        {
            try
            {
                var favorites = await _db.Favorites
                    .Where(f => f.UserId == userId)
                    .Include(f => f.product)
                    .ToListAsync();

                
                var products = favorites.Select(f => f.product).ToList();

                return Ok(new { status = true, products });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = err.Message });
            }
        }


        [HttpDelete("{userId}/{productId}")]
        public async Task<IActionResult> DeleteFromFavorites(int userId, int productId)
        {
            try
            {
                var favorite = await _db.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                if (favorite == null)
                {
                    return BadRequest(new { status = false, message = "Favorite not found" });
                }

                _db.Favorites.Remove(favorite);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Product deleted from favorites successfully" });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = err.Message });
            }
        }

        [HttpDelete("{userId}")]
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



    }
}
