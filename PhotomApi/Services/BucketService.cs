using Amazon.S3;
using Amazon.S3.Model;
using PhotomApi.Context;
using PhotomApi.Interfaces;
using PhotomApi.Models.Dto;
using PhotomApi.Services.Enums;
using System.Text.RegularExpressions;

namespace PhotomApi.Services
{
    public class BucketService : IBucketService
    {
        private readonly AmazonContext _amazonContext;
        private readonly ILogger<BucketService> _logger;
        public BucketService(AmazonContext amazonContext, ILogger<BucketService> logger)
        {
            _amazonContext = amazonContext;
            _logger = logger;
        }
        public async Task<ServiceResponseDto<DeleteObjectResponse>> DeleteFileAsync(string key)
        {
            var serviceResponse = new ServiceResponseDto<DeleteObjectResponse>();
            var fileExists = await CheckFileExistsAsync(key);

            if (!fileExists)
            {
                serviceResponse.Status = ServiceResponseStatus.NOT_FOUND;
                serviceResponse.ErrorMessage = $"File '{key}' doesn't exist on storage";
                return serviceResponse;
            }

            using IAmazonS3 client = _amazonContext.GetConnection();
            _logger.LogInformation($"Deleting file {key} on storage");

            try
            {
                var deleteResponse = await client.DeleteObjectAsync(_amazonContext.bucketName, $"{_amazonContext.rootPrefix}/{key}");
                serviceResponse.Status = ServiceResponseStatus.NO_CONTENT;
            }
            catch (Exception ex)
            {
                serviceResponse.Status = ServiceResponseStatus.INTERNAL_SERVER_ERROR;
                _logger.LogError($"Error while removing object on storage: Error => {ex.Message}, Stack Trace => {ex.Message}, File => {key}");
            }

            return serviceResponse;
        }

        public async Task<ServiceResponseDto<IList<S3ObjectDto>>> GetAllFilesAsync()
        {
            using IAmazonS3 client = _amazonContext.GetConnection();
            _logger.LogInformation("Getting all files from storage");

            var request = new ListObjectsV2Request()
            {
                BucketName = _amazonContext.bucketName,
                Prefix = _amazonContext.rootPrefix
            };

            var serviceResponse = new ServiceResponseDto<IList<S3ObjectDto>>();

            try
            {
                var objectsList = await client.ListObjectsV2Async(request);

                var s3Objects = objectsList.S3Objects.Select(o =>
                {
                    var urlRequest = new GetPreSignedUrlRequest()
                    {
                        BucketName = _amazonContext.bucketName,
                        Key = o.Key,
                        Expires = DateTime.UtcNow.AddHours(1)
                    };

                    return new S3ObjectDto()
                    {
                        Name = ExtractRootPrefix(o.Key.ToString()),
                        PresignedUrl = client.GetPreSignedURL(urlRequest)
                    };

                }).ToList();

                serviceResponse.Status = ServiceResponseStatus.SUCCESS;
                serviceResponse.Content = s3Objects;
            }
            catch (Exception ex)
            {
                serviceResponse.Status = ServiceResponseStatus.INTERNAL_SERVER_ERROR;
                _logger.LogError($"Error while getting all objects on storage: Error => {ex.Message}, Stack Trace => {ex.Message}");
            }

            return serviceResponse;
        }

        public async Task<bool> CheckFileExistsAsync(string key)
        {
            using IAmazonS3 client = _amazonContext.GetConnection();
            _logger.LogInformation($"Checking if file {key} exists on storage");

            try
            {
                var s3Object = await client.GetObjectAsync(_amazonContext.bucketName, $"{_amazonContext.rootPrefix}/{key}");
                _logger.LogInformation($"File {key} exists on storage");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while getting object on storage: Error => {ex.Message}, Stack Trace => {ex.Message}, File => {key}");
                return false;
            }

            return true;
        }

        public async Task<ServiceResponseDto<PutObjectResponse>> UploadFileAsync(IFormFile file)
        {
            var serviceResponse = new ServiceResponseDto<PutObjectResponse>();

            if (file == null)
            {
                serviceResponse.Status = ServiceResponseStatus.BAD_REQUEST;
                serviceResponse.ErrorMessage = "Unsupported file";
                return serviceResponse;
            }

            var isRightFileExtension = Regex.Match(file.ContentType, "(jpg|jpeg|png)", RegexOptions.IgnoreCase).Success;

            if (!isRightFileExtension)
            {
                serviceResponse.Status = ServiceResponseStatus.BAD_REQUEST;
                serviceResponse.ErrorMessage = "Only jpg/jpeg/png files are allowed";
            }

            var maxFileSize = 10485760;

            if (file.Length > maxFileSize)
            {
                serviceResponse.Status = ServiceResponseStatus.BAD_REQUEST;
                serviceResponse.ErrorMessage = "Only 10MB or lower is supported";
            }

            var fileExists = await CheckFileExistsAsync(file.FileName);

            if (fileExists)
            {
                serviceResponse.Status = ServiceResponseStatus.BAD_REQUEST;
                serviceResponse.ErrorMessage = $"File '{file.FileName}' already exists in storage";
            }

            if (serviceResponse.Status == ServiceResponseStatus.BAD_REQUEST)
            {
                return serviceResponse;
            }

            using IAmazonS3 client = _amazonContext.GetConnection();
            _logger.LogInformation($"Uploading file {file.FileName} to storage");

            var request = new PutObjectRequest()
            {
                BucketName = _amazonContext.bucketName,
                InputStream = file.OpenReadStream(),
                Key = $"{_amazonContext.rootPrefix}/{file.FileName}",
                ContentType = file.ContentType,
            };

            try
            {
                var putResponse = await client.PutObjectAsync(request);
                _logger.LogInformation($"Successfully uploaded file {file.FileName} to storage");

                serviceResponse.Status = ServiceResponseStatus.SUCCESS;
                serviceResponse.Content = putResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while uploading object on storage: Error => {ex.Message}, Stack Trace => {ex.Message}, File => {file.FileName}");
                serviceResponse.Status = ServiceResponseStatus.INTERNAL_SERVER_ERROR;
            }

            return serviceResponse;
        }

        private string ExtractRootPrefix(string key)
        {
            return key.Replace($"{_amazonContext.rootPrefix}/", "");
        }
    }
}
