using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserAPI.Models;

namespace UserAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IOptions<MongoDbSettings> mongoDbSettings)
    {
        try
        {
            var settings = MongoClientSettings.FromConnectionString(mongoDbSettings.Value.ConnectionString);

            // Configure timeouts and retry settings
            settings.ConnectTimeout = TimeSpan.FromSeconds(30);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
            settings.SocketTimeout = TimeSpan.FromSeconds(30);
            settings.MaxConnectionPoolSize = 100;

            var mongoClient = new MongoClient(settings);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _users = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to connect to MongoDB: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            return await _users.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving users: {ex.Message}", ex);
        }
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        try
        {
            return await _users.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving user by ID: {ex.Message}", ex);
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            return await _users.Find(x => x.Email == email).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving user by email: {ex.Message}", ex);
        }
    }

    public async Task<User> CreateAsync(User user)
    {
        try
        {
            await _users.InsertOneAsync(user);
            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating user: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateAsync(string id, User user)
    {
        try
        {
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _users.ReplaceOneAsync(x => x.Id == id, user);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating user: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var result = await _users.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error deleting user: {ex.Message}", ex);
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        try
        {
            var count = await _users.CountDocumentsAsync(x => x.Id == id);
            return count > 0;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking user existence: {ex.Message}", ex);
        }
    }
}