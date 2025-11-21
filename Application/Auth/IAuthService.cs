using Application.Auth.Dto;
using Infrastructure.Responses;

namespace Application.Auth;

public interface IAuthService
{
    Task<Response<string>> RegisterAsync(RegisterDto dto);
    Task<Response<AuthResultDto>> LoginAsync(LoginDto dto);
}


