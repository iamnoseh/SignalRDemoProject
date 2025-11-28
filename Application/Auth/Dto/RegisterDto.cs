using System.ComponentModel.DataAnnotations;

namespace Application.Auth.Dto;

public class RegisterDto
{
    [Required]
    public string Nickname { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
