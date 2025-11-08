using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserAPI.Models;

namespace UserAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IOptions<MongoDbSettings> mongoDbSettings, ILogger<UserRepository> logger)
    {
        _logger = logger;

        try
        {
            _logger.LogInformation("Initializing MongoDB connection to database: {DatabaseName}",
                mongoDbSettings.Value.DatabaseName);

            var settings = MongoClientSettings.FromConnectionString(mongoDbSettings.Value.ConnectionString);

            // Configure timeouts and retry settings
            settings.ConnectTimeout = TimeSpan.FromSeconds(30);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
            settings.SocketTimeout = TimeSpan.FromSeconds(30);
            settings.MaxConnectionPoolSize = 100;

            var mongoClient = new MongoClient(settings);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _users = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);

            _logger.LogInformation("Successfully initialized MongoDB connection to collection: {CollectionName}",
                mongoDbSettings.Value.UsersCollectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MongoDB. Database: {DatabaseName}, Collection: {CollectionName}",
                mongoDbSettings.Value.DatabaseName, mongoDbSettings.Value.UsersCollectionName);
            throw new InvalidOperationException($"Failed to connect to MongoDB: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all users from database");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var users = await _users.Find(_ => true).ToListAsync();
            stopwatch.Stop();

            _logger.LogInformation("Successfully retrieved {UserCount} users in {ElapsedMs}ms",
                users.Count, stopwatch.ElapsedMilliseconds);

            return users;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error retrieving users after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException($"Error retrieving users: {ex.Message}", ex);
        }
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving user by ID: {UserId}", id);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var user = await _users.Find(x => x.Id == id).FirstOrDefaultAsync();
            stopwatch.Stop();

            if (user != null)
            {
                _logger.LogInformation("Successfully retrieved user {UserId} in {ElapsedMs}ms",
                    id, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("User not found with ID: {UserId} (searched in {ElapsedMs}ms)",
                    id, stopwatch.ElapsedMilliseconds);
            }

            return user;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error retrieving user by ID: {UserId} after {ElapsedMs}ms",
                id, stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException($"Error retrieving user by ID: {ex.Message}", ex);
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Retrieving user by email: {Email}", email);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var user = await _users.Find(x => x.Email == email).FirstOrDefaultAsync();
            stopwatch.Stop();

            if (user != null)
            {
                _logger.LogInformation("Successfully retrieved user by email {Email} (ID: {UserId}) in {ElapsedMs}ms",
                    email, user.Id, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("User not found with email: {Email} (searched in {ElapsedMs}ms)",
                    email, stopwatch.ElapsedMilliseconds);
            }

            return user;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error retrieving user by email: {Email} after {ElapsedMs}ms",
                email, stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException($"Error retrieving user by email: {ex.Message}", ex);
        }
    }

    public async Task<User> CreateAsync(User user)
    {
        _logger.LogInformation("Creating new user with email: {Email}", user.Email);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _users.InsertOneAsync(user);
            stopwatch.Stop();

            _logger.LogInformation("Successfully created user {UserId} with email {Email} in {ElapsedMs}ms",
                user.Id, user.Email, stopwatch.ElapsedMilliseconds);

            return user;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error creating user with email: {Email} after {ElapsedMs}ms",
                user.Email, stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException($"Error creating user: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateAsync(string id, User user)
    {
        _logger.LogInformation("Updating user: {UserId}", id);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _users.ReplaceOneAsync(x => x.Id == id, user);
            stopwatch.Stop();

            var success = result.ModifiedCount > 0;

            if (success)
            {
                _logger.LogInformation("Successfully updated user {UserId} in {ElapsedMs}ms",
                    id, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("No user was updated for ID: {UserId} (operation took {ElapsedMs}ms)",
                    id, stopwatch.ElapsedMilliseconds);
            }

            return success;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error updating user: {UserId} after {ElapsedMs}ms",
                id, stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException($"Error updating user: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting user: {UserId}", id);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await _users.DeleteOneAsync(x => x.Id == id);
            stopwatch.Stop();

            var success = result.DeletedCount > 0;

            if (success)
            {
                _logger.LogInformation("Successfully deleted user {UserId} in {ElapsedMs}ms",
                    id, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("No user was deleted for ID: {UserId} (operation took {ElapsedMs}ms)",
                    id, stopwatch.ElapsedMilliseconds);
            }

            return success;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error deleting user: {UserId} after {ElapsedMs}ms",
                id, stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException($"Error deleting user: {ex.Message}", ex);
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        _logger.LogInformation("Checking if user exists: {UserId}", id);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var count = await _users.CountDocumentsAsync(x => x.Id == id);
            stopwatch.Stop();

            var exists = count > 0;

            _logger.LogInformation("User existence check for {UserId}: {Exists} (checked in {ElapsedMs}ms)",
                id, exists, stopwatch.ElapsedMilliseconds);

            return exists;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error checking user existence: {UserId} after {ElapsedMs}ms",
                id, stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException($"Error checking user existence: {ex.Message}", ex);
        }
    }
}