using System.ComponentModel.DataAnnotations;

namespace PhotomApi.Models.Dto
{
    public class UserLoginDto
    {
        [Required]
        public string? ClientID { get; set; }

        [Required]
        public string? ClientSecret { get; set; }
    }
}
