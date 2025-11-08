using UserAPI.DTOs;
using UserAPI.Models;
using UserAPI.Repositories;

namespace UserAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToResponseDto);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToResponseDto(user) : null;
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Check if user with this email already exists
        var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash password (in production, use proper password hashing)
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        var user = new User
        {
            Name = createUserDto.Name,
            Email = createUserDto.Email,
            Password = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);
        return MapToResponseDto(createdUser);
    }

    public async Task<UserResponseDto?> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
    {
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
        {
            return null;
        }

        // Check if email is being changed and if it's already taken by another user
        if (existingUser.Email != updateUserDto.Email)
        {
            var userWithEmail = await _userRepository.GetByEmailAsync(updateUserDto.Email);
            if (userWithEmail != null && userWithEmail.Id != id)
            {
                throw new InvalidOperationException("Email is already taken by another user");
            }
        }

        existingUser.Name = updateUserDto.Name;
        existingUser.Email = updateUserDto.Email;
        existingUser.UpdatedAt = DateTime.UtcNow;

        var updated = await _userRepository.UpdateAsync(id, existingUser);
        return updated ? MapToResponseDto(existingUser) : null;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public async Task<bool> UserExistsAsync(string id)
    {
        return await _userRepository.ExistsAsync(id);
    }

    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id!,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}