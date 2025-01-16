namespace Application.Core
{
    public interface IImagesCloudService<T>
    {
        Task<T> UploadImage<U>(U file) where U : IFormFile;
    }
    
};

