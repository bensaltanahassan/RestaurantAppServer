using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Interfaces;
using RestaurantAppServer.Models;

namespace RestaurantAppServer.Controllers
{
    [Authorize(Roles = "user")]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        private readonly AppDbContext _db;
        private readonly IImageService _imageService;

        public CategoryController(AppDbContext db, IImageService imageService)
        {
            _db = db;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _db.Categories.Include(c => c.image).ToListAsync();
                return Ok(new { status = true, categories });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = e.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromForm] CategoryModel cm, IFormFile file)
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
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message });
            }
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] CategoryModel cm, IFormFile? file)
        {
            try
            {
                var category = await _db.Categories.Include(c => c.image).FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                {
                    return NotFound(new { status = false, message = "Category not found" });
                }

                // Update category properties if provided
                if (cm.Name != null) category.Name = cm.Name;
                if (cm.NameAn != null) category.NameAn = cm.NameAn;

                // Handle image upload if provided
                if (file != null)
                {
                    Image? oldImage = null;
                    if (category.image != null)
                    {
                        // Delete old image from cloud
                        var resultOldImage = await _imageService.DeleteImageAsync(category.image.PublicId);
                        if (resultOldImage.Error != null)
                            return BadRequest(new { status = false, message = "Failed to delete old image" });
                        //assigne the image to oldImage
                        oldImage = category.image;
                    }

                    // Upload new image
                    var result = await _imageService.AddImageAsync(file);
                    if (result.Error != null)
                        return BadRequest(new { status = false, message = "Failed to upload new image" });

                    // Create new image record
                    Image image = new()
                    {
                        PublicId = result.PublicId,
                        Url = result.SecureUrl.AbsoluteUri
                    };
                    await _db.Images.AddAsync(image);
                    await _db.SaveChangesAsync();


                    // Update category with new image
                    category.image = image;

                    //delete oldImage from db
                    if (oldImage != null) _db.Images.Remove(oldImage);

                }

                // Update category in the database
                _db.Categories.Update(category);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Category updated successfully" });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = err.Message, details = err.InnerException?.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _db.Categories.Include(c => c.image).FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                {
                    return NotFound(new { status = false, message = "Category not found" });
                }
                if (category.image != null)
                {
                    // Delete image from cloud
                    var resultOldImage = await _imageService.DeleteImageAsync(category.image.PublicId);
                    if (resultOldImage.Error != null)
                        return BadRequest(new { status = false, message = "Failed to delete old image" });
                    _db.Images.Remove(category.image);
                }
                _db.Categories.Remove(category);

                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Category deleted with success" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = e.Message });
            }
        }


    }
}
