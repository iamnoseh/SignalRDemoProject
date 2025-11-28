using System.ComponentModel.DataAnnotations;

namespace Application.Auth.Dto;

public class LoginDto
{
    [Required]
    public string Identifier { get; set; } = string.Empty; // Phone or Nickname

    [Required]
    public string Password { get; set; } = string.Empty;
}
