using PhotomApi.Services.Enums;

namespace PhotomApi.Models.Dto
{
    public class ServiceResponseDto<T>
    {
        public ServiceResponseStatus Status { get; set; }
        public T? Content { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
