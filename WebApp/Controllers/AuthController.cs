using Application.Auth;
using Application.Auth.Dto;
using Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<Response<string>>> Register([FromBody] RegisterDto request)
    {
        var result = await authService.RegisterAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Response<AuthResultDto>>> Login([FromBody] LoginDto request)
    {
        var result = await authService.LoginAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<Response<bool>>> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await authService.UpdateProfileAsync(userId, request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost("profile/picture")]
    public async Task<ActionResult<Response<string>>> UpdateProfilePicture(IFormFile file)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Create profiles directory
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        // Generate unique filename
        var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"/uploads/profiles/{fileName}";
        var result = await authService.UpdateProfilePictureAsync(userId, fileUrl);
        return StatusCode(result.StatusCode, result);
    }
}
