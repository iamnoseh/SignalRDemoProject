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
    public async Task<ActionResult<Response<AuthResultDto>>> Register([FromBody] RegisterDto request)
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
}
