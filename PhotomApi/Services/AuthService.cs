using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PhotomApi.Config;
using PhotomApi.Interfaces;
using PhotomApi.Models;
using PhotomApi.Models.Dto;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace PhotomApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtCredentialOptions _jwtOptions;
        private readonly ILogger<AuthService> _logger;
        public AuthService(IOptions<JwtCredentialOptions> jwtOptions, ILogger<AuthService> logger)
        {
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }
        public User Authenticate(UserLoginDto user)
        {
            _logger.LogInformation("Trying to authenticate user");
            if (user.ClientID != _jwtOptions.ClientID || user.ClientSecret != _jwtOptions.ClientSecret)
            {
                _logger.LogInformation("User not found");
                return null;
            }

            _logger.LogInformation("User authenticated successfully");
            return new User() { ClientID = user.ClientID, ClientSecret = user.ClientSecret };
        }

        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_jwtOptions.Issuer,
                _jwtOptions.Audience,
                null,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
