using RealTimeChatApp.DTOs;
using RealTimeChatApp.Models;

namespace RealTimeChatApp.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<string?> LoginAsync(UserDto request);
    }
}
