using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserAPI.DTOs;
using UserAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace UserAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of users</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), 200)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
    {
        var correlationId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Getting all users - CorrelationId: {CorrelationId}", correlationId);

        try
        {
            var users = await _userService.GetAllUsersAsync();
            _logger.LogInformation("Successfully retrieved {UserCount} users - CorrelationId: {CorrelationId}",
                users.Count(), correlationId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users - CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving users",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="400">Invalid user ID format</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<UserResponseDto>> GetUserById([Required] string id)
    {
        var correlationId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Getting user by ID: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);

        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("Invalid user ID provided - CorrelationId: {CorrelationId}", correlationId);
            return BadRequest(new { message = "User ID is required", correlationId });
        }

        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
                return NotFound(new { message = "User not found", correlationId });
            }

            _logger.LogInformation("Successfully retrieved user: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving the user",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="createUserDto">User creation data</param>
    /// <returns>Created user</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="409">User with email already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 409)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var correlationId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Creating new user with email: {Email} - CorrelationId: {CorrelationId}",
            createUserDto?.Email, correlationId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for user creation - CorrelationId: {CorrelationId}", correlationId);
            return BadRequest(new
            {
                message = "Invalid input data",
                errors = ModelState,
                correlationId
            });
        }

        try
        {
            var user = await _userService.CreateUserAsync(createUserDto);
            _logger.LogInformation("Successfully created user: {UserId} - CorrelationId: {CorrelationId}",
                user.Id, correlationId);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict while creating user - CorrelationId: {CorrelationId}", correlationId);
            return Conflict(new { message = ex.Message, correlationId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user - CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, new
            {
                message = "An error occurred while creating the user",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Update user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="updateUserDto">User update data</param>
    /// <returns>Updated user</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="404">User not found</response>
    /// <response code="409">Email conflict</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 409)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<UserResponseDto>> UpdateUser([Required] string id, [FromBody] UpdateUserDto updateUserDto)
    {
        var correlationId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Updating user: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);

        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "User ID is required", correlationId });
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for user update - CorrelationId: {CorrelationId}", correlationId);
            return BadRequest(new
            {
                message = "Invalid input data",
                errors = ModelState,
                correlationId
            });
        }

        try
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            if (user == null)
            {
                _logger.LogWarning("User not found for update: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
                return NotFound(new { message = "User not found", correlationId });
            }

            _logger.LogInformation("Successfully updated user: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict while updating user: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
            return Conflict(new { message = ex.Message, correlationId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
            return StatusCode(500, new
            {
                message = "An error occurred while updating the user",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Delete user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">User deleted successfully</response>
    /// <response code="400">Invalid user ID</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult> DeleteUser([Required] string id)
    {
        var correlationId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Deleting user: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);

        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "User ID is required", correlationId });
        }

        try
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("User not found for deletion: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
                return NotFound(new { message = "User not found", correlationId });
            }

            _logger.LogInformation("Successfully deleted user: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
            return Ok(new
            {
                message = "User deleted successfully",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
            return StatusCode(500, new
            {
                message = "An error occurred while deleting the user",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Check if user exists
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>HTTP 200 if exists, HTTP 404 if not found</returns>
    /// <response code="200">User exists</response>
    /// <response code="400">Invalid user ID</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpHead("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult> UserExists([Required] string id)
    {
        var correlationId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Checking if user exists: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);

        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "User ID is required", correlationId });
        }

        try
        {
            var exists = await _userService.UserExistsAsync(id);
            if (exists)
            {
                _logger.LogInformation("User exists: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
                return Ok();
            }
            else
            {
                _logger.LogInformation("User does not exist: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user existence: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
            return StatusCode(500, new
            {
                message = "An error occurred while checking user existence",
                correlationId,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Health check endpoint for AWS Lambda
    /// </summary>
    /// <returns>Health status</returns>
    /// <response code="200">Service is healthy</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult<object> HealthCheck()
    {
        var correlationId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Health check requested - CorrelationId: {CorrelationId}", correlationId);

        return Ok(new
        {
            status = "healthy",
            service = "UserAPI",
            timestamp = DateTime.UtcNow,
            correlationId,
            version = "1.0.0"
        });
    }
}
