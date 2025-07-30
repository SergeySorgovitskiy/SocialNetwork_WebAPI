# SocialNetwork


##  Project Description

SocialNetwork is a modern RESTful API for a social media platform built with .NET 9 using Clean Architecture principles. The application provides comprehensive social networking features including user authentication, content sharing, social interactions, and real-time communication capabilities.

##  Features

### Authentication & Authorization

- **JWT-based authentication** with secure token management
- **User registration and login** with email validation
- **Role-based access control** for different user permissions
- **Refresh token mechanism** for enhanced security

### User Management

- **User profiles** with customizable avatars and bios
- **Privacy settings** (public/private profiles)
- **User search and discovery** functionality
- **Profile customization** with bio and avatar support

### Content Management

- **Post creation and editing** with rich text support
- **Media upload support** for images and videos
- **Hashtag system** for content categorization
- **Content moderation** and filtering capabilities

###  Social Interactions

- **Like/Unlike posts** with real-time counters
- **Comment system** with threaded discussions
- **Repost functionality** with attribution
- **Bookmark posts** for later reading
- **Follow/Unfollow users** with subscription management

###  News Feed

- **Personalized feed** based on user subscriptions
- **Content filtering** by various criteria
- **Real-time updates** for new posts and interactions
- **Trending content** discovery

### Advanced Features

- **Search functionality** across users and posts
- **Notification system** for social interactions
- **Content analytics** and engagement metrics
- **API rate limiting** and security measures

##  Technology Stack

### Backend

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 9.0** - ORM for database operations
- **SQL Server** - Primary database
- **JWT Authentication** - Secure token-based authentication
- **Swagger/OpenAPI** - API documentation and testing

### Architecture

- **Clean Architecture** - Separation of concerns with layered architecture
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - IoC container for service management
- **CQRS Pattern** - Command Query Responsibility Segregation
- **Unit of Work Pattern** - Transaction management

## Architecture Overview

### Domain Layer

- **Entities**: Core business objects (User, Post, Comment, Like, etc.)
- **Interfaces**: Contracts for repositories and services
- **Enums**: Domain-specific enumerations
- **Common**: Shared domain utilities and exceptions

### Application Layer

- **Services**: Business logic implementation
- **Common**: Application-wide configurations and utilities
- **DTOs**: Data transfer objects for API communication
- **Validators**: Input validation and business rules

### Infrastructure Layer

- **Data**: Database context and configurations
- **Repositories**: Data access implementations
- **Migrations**: Database schema management
- **External Services**: Third-party integrations

### Web API Layer

- **Controllers**: HTTP endpoint handlers
- **Middleware**: Request/response processing
- **Configuration**: Application settings and startup
- **Authentication**: JWT token handling

## Project Structure

```
SocialNetwork/
├── Domain/                 # Core business logic
│   ├── Entities/          # Domain entities
│   ├── Interfaces/        # Repository contracts
│   ├── Enums/            # Domain enumerations
│   └── Common/           # Shared utilities
├── Application/           # Business logic layer
│   ├── Services/         # Application services
│   └── Common/           # Application utilities
├── Infrastructure/        # Data access layer
│   ├── Data/             # Database context
│   ├── Repositories/     # Repository implementations
│   └── Migrations/       # Database migrations
├── WebApi/               # API presentation layer
│   ├── Controllers/      # API endpoints
│   ├── wwwroot/          # Static files
│   └── Properties/       # API configuration
└── tests/                # Test projects
    ├── UnitTests/        # Unit tests
    └── IntegrationTests/ # Integration tests
```

### Setup Instructions

1. **Clone the repository**

   ```bash
   git clone https://github.com/yourusername/SocialNetwork.git
   cd SocialNetwork
   ```

2. **Configure the database**

   - Update the connection string in `WebApi/appsettings.json`
   - Ensure SQL Server is running and accessible

3. **Apply database migrations**

   ```bash
   cd WebApi
   dotnet ef database update
   ```

4. **Build and run the project**

   ```bash
   dotnet build
   dotnet run --project WebApi
   ```

5. **Access the API**
   - API: `https://localhost:7001`
   - Swagger UI: `https://localhost:7001/swagger`

### Environment Configuration

Update `WebApi/appsettings.json` with your settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SocialNetworkDb;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-here",
    "Issuer": "SocialNetwork",
    "Audience": "SocialNetworkUsers",
    "ExpiryInMinutes": 60
  }
}
```

## API Documentation

The API is fully documented using Swagger/OpenAPI. Once the application is running, visit:

- **Swagger UI**: `https://localhost:7001/swagger`
- **API Endpoints**:
  - `POST /api/auth/register` - User registration
  - `POST /api/auth/login` - User authentication
  - `GET /api/users` - Get users
  - `POST /api/posts` - Create post
  - `GET /api/posts` - Get posts
  - `POST /api/posts/{id}/like` - Like post
  - `POST /api/posts/{id}/comment` - Comment on post
  - `POST /api/users/{id}/follow` - Follow user
  - `GET /api/newsfeed` - Get personalized feed

## Testing

### Unit Tests

```bash
dotnet test tests/UnitTests/
```

### Integration Tests

```bash
dotnet test tests/IntegrationTests/
```

### Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName --project Infrastructure --startup-project WebApi

# Update database
dotnet ef database update --project Infrastructure --startup-project WebApi
```


