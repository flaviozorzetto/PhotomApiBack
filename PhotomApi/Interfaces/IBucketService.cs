using Amazon.S3.Model;
using PhotomApi.Models.Dto;

namespace PhotomApi.Interfaces
{
    public interface IBucketService
    {
        Task<ServiceResponseDto<PutObjectResponse>> UploadFileAsync(IFormFile file);
        Task<ServiceResponseDto<IList<S3ObjectDto>>> GetAllFilesAsync();
        Task<bool> CheckFileExistsAsync(string key);
        Task<ServiceResponseDto<DeleteObjectResponse>> DeleteFileAsync(string key);
    }
}
