namespace Application.Users.Dto;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? UserName { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
