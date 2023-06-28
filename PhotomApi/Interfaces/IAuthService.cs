using PhotomApi.Models;
using PhotomApi.Models.Dto;

namespace PhotomApi.Interfaces
{
    public interface IAuthService
    {
        string GenerateToken(User user);
        User Authenticate(UserLoginDto user);
    }
}
