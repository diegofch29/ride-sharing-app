using UserAPI.DTOs;
using UserAPI.Models;

namespace UserAPI.Services;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(string id);
    Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserResponseDto?> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(string id);
    Task<bool> UserExistsAsync(string id);
}