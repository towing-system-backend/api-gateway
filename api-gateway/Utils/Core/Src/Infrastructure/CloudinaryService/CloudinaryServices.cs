using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Application.Core
{
    public class CloudinaryServices : IImagesCloudService<string>
    {
        public async Task<string> UploadImage<U>(U file) where U : IFormFile
        {
            var cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "Images",
                    Transformation = new Transformation()
                        .Width(500)
                        .Height(500)
                        .Crop("fill")
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    throw new Exception($"Error uploading the image: {uploadResult.Error.Message}");
                return uploadResult.SecureUrl.ToString();
            }
        }
    }
}
    


