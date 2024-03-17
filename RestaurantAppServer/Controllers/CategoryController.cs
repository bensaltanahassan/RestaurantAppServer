using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Interfaces;
using RestaurantAppServer.Models;

namespace RestaurantAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        private readonly AppDbContext _db;
        private readonly IImageService _imageService;

        public CategoryController(AppDbContext db,IImageService imageService)
        {
            _db = db;
            _imageService = imageService;
        }

        /**
         * @description     Get all categories
         * @router          /
         * @method          GET
         * @access          public
         */
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _db.Categories.ToListAsync();
                return Ok(new { status = true, categories });
            }
            catch
            {
                return BadRequest(new { status = false, message = "Internal Server Error" });
            }
        }


        /**
         * @description     Create New category
         * @router          /
         * @method          POST
         * @access          private(only admin)
         */
        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CategoryModel cm,
            IFormFile file
        )
        {
            try
            {
                
                
                var result = await _imageService.AddImageAsync(file);
                if (result.Error!=null)
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
                Category category = new()
                {
                    Name = cm.Name,
                    NameAn = cm.NameAn,
                    ImageId = image.Id,
                };
                await _db.Categories.AddAsync(category);
                
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Category created with success" });
            }
            catch
            {
                return BadRequest(new { status = false, message = "Internal Server Error" });
            }
        }

        /**
         * @description     Update category
         * @router          /
         * @method          PUT
         * @access          private(only admin)
         */
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryModel cm)
        {
            try
            {
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                {
                    return BadRequest(new { status = false, message = "Category not found" });
                }
                category.Name = cm.Name;
                category.NameAn = cm.NameAn;
                category.ImageId = cm.ImageId;
                _db.Categories.Update(category);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Category updated with success" });
            }
            catch
            {
                return BadRequest(new { status = false, message = "Internal Server Error" });
            }
        }

        /**
        * @description     DELETE Category
        * @router          /api/categories/:id
        * @method          DELETE
        * @access          private(only admin)
        */
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                {
                    return BadRequest(new { status = false, message = "Category not found" });
                }
                _db.Categories.Remove(category);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Category deleted with success" });
            }
            catch
            {
                return BadRequest(new { status = false, message = "Internal Server Error" });
            }
        }


    }
}
