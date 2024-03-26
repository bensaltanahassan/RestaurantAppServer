using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using RestaurantAppServer.Interfaces;
using RestaurantAppServer.Utils;

namespace RestaurantAppServer.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        public ImageService(IOptions<CloudinarySettings> configs)
        {
            var acc  = new Account
            {
                Cloud = configs.Value.CloudName,
                ApiKey = configs.Value.ApiKey,
                ApiSecret = configs.Value.ApiSecret
            };
            _cloudinary = new Cloudinary(acc);
        }   

        public async Task<ImageUploadResult> AddImageAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();
            if(file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                stream.Position = 0;
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            return uploadResult;
        }

        public Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = _cloudinary.DestroyAsync(deleteParams);
            return result;
        }
    }
}
