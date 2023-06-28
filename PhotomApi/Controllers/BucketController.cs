using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotomApi.Interfaces;
using PhotomApi.Services.Enums;

namespace PhotomApi.Controllers
{
    [Authorize]
    public class BucketController : Controller
    {
        private readonly IBucketService _bucketService;
        public BucketController(IBucketService bucketService)
        {
            _bucketService = bucketService;
        }


        [HttpGet("bucket")]
        public async Task<IActionResult> GetAllFiles()
        {
            var serviceResponse = await _bucketService.GetAllFilesAsync();

            if (serviceResponse.Status == ServiceResponseStatus.INTERNAL_SERVER_ERROR)
            {
                return BadRequest("Unknown error in server-side");
            }

            return Ok(serviceResponse.Content);
        }

        [HttpPost("bucket")]
        public async Task<IActionResult> InsertNewFile(IFormFile file)
        {
            var serviceResponse = await _bucketService.UploadFileAsync(file);

            if (serviceResponse.Status == ServiceResponseStatus.BAD_REQUEST)
            {
                return BadRequest(serviceResponse.ErrorMessage);
            }

            if (serviceResponse.Status == ServiceResponseStatus.INTERNAL_SERVER_ERROR)
            {
                return BadRequest("Unknown error in server");
            }

            return Ok($"File '{file.FileName}' successfully uploaded to S3");
        }

        [HttpDelete("bucket/{key}")]
        public async Task<IActionResult> DeleteFile(string key)
        {
            var serviceResponse = await _bucketService.DeleteFileAsync(key);

            if (serviceResponse.Status == ServiceResponseStatus.NOT_FOUND)
            {
                return NotFound(serviceResponse.ErrorMessage);
            }

            return NoContent();
        }
    }
}
