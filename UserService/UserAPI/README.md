# UserAPI - CRUD Service for AWS Lambda

A .NET 10 Web API providing CRUD operations for user management, designed to run on AWS Lambda with MongoDB as the data store.

## Features

- Complete CRUD operations for users
- MongoDB integration with connection string configuration
- Password hashing using BCrypt
- Email validation and uniqueness checks
- AWS Lambda optimized with containerized deployment
- CORS enabled for cross-origin requests
- Comprehensive error handling and validation

## API Endpoints

### Users

- `GET /api/user` - Get all users
- `GET /api/user/{id}` - Get user by ID
- `POST /api/user` - Create new user
- `PUT /api/user/{id}` - Update user
- `DELETE /api/user/{id}` - Delete user
- `HEAD /api/user/{id}` - Check if user exists

### Request/Response Examples

#### Create User

```json
POST /api/user
{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "password": "securepassword123"
}
```

#### Response

```json
{
  "id": "67390a2b8f1e2a4567890123",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "createdAt": "2025-11-07T12:00:00Z",
  "updatedAt": "2025-11-07T12:00:00Z"
}
```

## Technologies Used

- **.NET 10** - Latest preview version
- **ASP.NET Core** - Web API framework
- **MongoDB.Driver** - MongoDB connectivity
- **BCrypt.Net** - Password hashing
- **Amazon.Lambda.AspNetCoreServer.Hosting** - AWS Lambda integration
- **Docker** - Containerization for AWS Lambda

## Configuration

### MongoDB Settings

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://mongoadmin:mongopasswd!!@54.235.41.240:27017/?authSource=admin",
    "DatabaseName": "UserServiceDB",
    "UsersCollectionName": "Users"
  }
}
```

## Development

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- MongoDB access

### Running Locally

```bash
dotnet run
```

### Running with Docker

```powershell
.\build-and-run.ps1
```

## Deployment

### AWS Lambda Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed deployment instructions.

### Key Steps:

1. Configure GitHub secrets
2. Push to main branch
3. GitHub Actions will automatically build and deploy

### Required AWS Resources:

- ECR Repository
- Lambda Function
- IAM Execution Role
- API Gateway (optional)

## Project Structure

```
UserAPI/
├── Controllers/
│   └── UserController.cs          # Main API controller
├── DTOs/
│   ├── CreateUserDto.cs           # User creation model
│   ├── UpdateUserDto.cs           # User update model
│   └── UserResponseDto.cs         # User response model
├── Models/
│   ├── User.cs                    # User entity
│   └── MongoDbSettings.cs         # MongoDB configuration
├── Repositories/
│   ├── IUserRepository.cs         # Repository interface
│   └── UserRepository.cs          # MongoDB repository implementation
├── Services/
│   ├── IUserService.cs            # Service interface
│   └── UserService.cs             # Business logic service
├── .github/workflows/
│   └── deploy.yml                 # GitHub Actions workflow
├── Dockerfile                     # Multi-stage Docker build
├── Program.cs                     # Application entry point
└── appsettings.*.json            # Configuration files
```

## Error Handling

The API includes comprehensive error handling for:

- Validation errors (400 Bad Request)
- Resource not found (404 Not Found)
- Duplicate email conflicts (409 Conflict)
- Server errors (500 Internal Server Error)

## Security Features

- Password hashing with BCrypt
- Email validation
- Input validation with data annotations
- CORS configuration for secure cross-origin requests

## MongoDB Integration

- Async operations for better performance
- Connection pooling and timeout configuration
- Proper error handling and logging
- Support for connection string configuration
