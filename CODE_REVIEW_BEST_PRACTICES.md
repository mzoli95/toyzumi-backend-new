# Code Review - Best Practices and Recommendations

## 🚨 CRITICAL SECURITY ISSUES (FIXED)

### 1. ✅ Hardcoded Credentials in EmailService
**Status**: FIXED
**Severity**: CRITICAL
**Location**: `kz-webshop-be/Services/EmailService.cs`

**Issue**: Email credentials were hardcoded directly in the source code, including email address and password.

**Fix Applied**: 
- Moved credentials to configuration system
- Added dependency injection for IConfiguration
- Updated appsettings.json with placeholders
- Email credentials should now be stored in User Secrets (development) or Environment Variables (production)

**Action Required**:
```bash
# For development, use user secrets:
dotnet user-secrets set "Email:FromEmail" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-app-password"

# For production, set environment variables:
# Email__FromEmail=your-email@gmail.com
# Email__Password=your-app-password
```

---

## 📋 BEST PRACTICES RECOMMENDATIONS

### Architecture & Design Patterns

#### 1. Service Layer Missing
**Current State**: Controllers directly inject `ApplicationDbContext` alongside repositories.
**Location**: `ProductController.cs`, `UserController.cs`

**Recommendation**:
- Implement a proper Service Layer between Controllers and Repositories
- Controllers should only depend on Services, not DbContext directly
- Services should orchestrate business logic and repository calls

**Example Structure**:
```csharp
// Add IProductService
public interface IProductService
{
    Task<IEnumerable<PromotionItemDto>> GetTopFavoritesAsync(Guid? userId);
    Task<ProductDto> GetProductByIdAsync(Guid id);
}

// ProductController should only inject services
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IEnumService _enumService;
    
    // No direct DbContext injection
}
```

**Benefits**:
- Better separation of concerns
- Easier to test
- Cleaner controller code
- Business logic centralized

---

#### 2. Repository Pattern Incomplete
**Current State**: Some repositories exist but don't follow a consistent pattern.
**Location**: All `*Repository.cs` files

**Recommendations**:

a) **Use Generic Repository Base Class**:
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<int> SaveChangesAsync();
}
```

b) **Separate Unit of Work**:
Currently, repositories directly save changes. Consider implementing Unit of Work pattern:
```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    IFunkoPopRepository FunkoPops { get; }
    Task<int> CommitAsync();
}
```

c) **Issue in UserRepository**: 
- Line 49 has Hungarian comment: "Próbáld ki, melyik claimben van a Firebase UID"
- Consider using English for all code comments
- Lines 92, 119: Hungarian comments present

---

### Error Handling & Logging

#### 3. Inconsistent Error Handling
**Current State**: Basic exception handling exists but is inconsistent.

**Recommendations**:

a) **Global Exception Handling Middleware**:
```csharp
// Add in Program.cs
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            // Log error
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "An error occurred",
                message = error.Error.Message 
            });
        }
    });
});
```

b) **Add Structured Logging**:
- Inject `ILogger<T>` into services and repositories
- Log important operations, errors, and performance metrics
- Consider using Serilog for better structured logging

**Example**:
```csharp
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            _logger.LogInformation("Sending email to {Recipient} with subject {Subject}", to, subject);
            // ... send email
            _logger.LogInformation("Email sent successfully to {Recipient}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", to);
            throw;
        }
    }
}
```

c) **Custom Exceptions**:
Create domain-specific exceptions:
```csharp
public class ProductNotFoundException : Exception
{
    public Guid ProductId { get; }
    public ProductNotFoundException(Guid productId) 
        : base($"Product with ID {productId} was not found")
    {
        ProductId = productId;
    }
}
```

---

### Security Improvements

#### 4. Authentication & Authorization
**Current State**: JWT authentication is configured but authorization is not consistently applied.

**Recommendations**:

a) **Add Authorization Attributes**:
```csharp
[Authorize] // Require authentication for all endpoints
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    [AllowAnonymous] // Public endpoints
    [HttpGet("search")]
    public async Task<ActionResult> SearchProducts()
    
    [Authorize(Roles = "Admin")] // Admin only
    [HttpPost]
    public async Task<ActionResult> CreateProduct()
}
```

b) **JWT Settings in Configuration**:
Move hardcoded JWT settings to configuration:
- Line 50-58 in Program.cs: "https://securetoken.google.com/zeem-funko" should be in config

c) **CORS Configuration**:
- Line 39-43 in Program.cs: CORS allows any method/header from localhost:4200
- Consider being more restrictive in production
- Move to configuration for different environments

---

#### 5. Input Validation
**Current State**: Minimal input validation observed.

**Recommendations**:

a) **Add Data Annotations to DTOs**:
```csharp
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
    
    [Phone]
    public string? Phone { get; set; }
}
```

b) **Enable Model Validation**:
```csharp
// In controllers
if (!ModelState.IsValid)
{
    return BadRequest(ModelState);
}
```

c) **Consider FluentValidation**:
For complex validation scenarios, consider using FluentValidation library.

---

#### 6. SQL Injection Protection
**Current State**: Good - using Entity Framework Core properly.
**Status**: ✅ No issues found

EF Core uses parameterized queries, which protects against SQL injection.

---

### Database & Performance

#### 7. Database Connection String
**Current State**: Connection string in appsettings.json
**Location**: `appsettings.json` line 9

**Recommendation**:
- Move to User Secrets for development
- Use Environment Variables or Azure Key Vault for production
- Never commit connection strings to source control

```bash
# Development
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

---

#### 8. Missing Database Indexes
**Current State**: Some indexes exist but could be optimized.
**Location**: `ApplicationDbContext.cs`

**Recommendations**:

Add composite indexes for common queries:
```csharp
// In ApplicationDbContext.OnModelCreating
modelBuilder.Entity<Product>()
    .HasIndex(p => new { p.IsActive, p.IsVisible, p.Stock });

modelBuilder.Entity<LikedItem>()
    .HasIndex(li => new { li.UserId, li.ProductId })
    .IsUnique();

modelBuilder.Entity<ShoppingCartItem>()
    .HasIndex(sci => new { sci.UserId, sci.ProductId });
```

---

#### 9. N+1 Query Problem
**Current State**: Potential N+1 issues in several places.

**Example Issues**:

a) `ProductController.cs` lines 50-58:
```csharp
// Two separate queries - could be optimized
var funkos = await _context.FunkoPops
    .Where(f => funkoIds.Contains(f.Id))
    .ToListAsync();

var labubus = await _context.Labubus
    .Where(l => labubuIds.Contains(l.Id))
    .ToListAsync();
```

**Recommendation**: Consider eager loading with `.Include()` where relationships are always needed.

---

#### 10. Async/Await Patterns
**Current State**: Generally good, but some improvements possible.

**Recommendations**:

a) **ConfigureAwait for Library Code**:
```csharp
// In libraries (not controllers)
await _context.SaveChangesAsync().ConfigureAwait(false);
```

b) **Avoid Async Void**:
All async methods should return Task or Task<T>, never void (except event handlers).

---

### Code Quality & Maintainability

#### 11. Magic Strings and Numbers
**Current State**: Some magic values scattered in code.

**Examples**:
- `ProductController.cs` line 103: Recently viewed max count `5` is hardcoded
- Various string literals for product types, availability states

**Recommendations**:

a) **Use Constants**:
```csharp
public static class ProductConstants
{
    public const int MaxRecentlyViewedItems = 5;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}
```

b) **Use Enums** (already partially done):
- Good use of `ProductType`, `RoleState`, `Badge` enums
- Consider enum for availability states ("instock", "preorder", etc.)

---

#### 12. Code Duplication
**Current State**: Significant duplication in ProductController.

**Example**: Lines 60-72 and 67-71 have similar mapping logic for DTOs.

**Recommendation**:
- Use AutoMapper consistently (already configured but not used everywhere)
- Extract common mapping logic to helper methods
- Consider extension methods for common transformations

```csharp
// Example
public static class ProductExtensions
{
    public static FunkoPopDto ToDto(this FunkoPop funkoPop, bool isFavorite = false)
    {
        return new FunkoPopDto
        {
            // mapping logic
            IsFavorite = isFavorite
        };
    }
}
```

---

#### 13. Large Controllers
**Current State**: `ProductController.cs` is 1000+ lines.

**Recommendation**:
- Split into multiple controllers (FunkoPopController, LabubuController already exist)
- Move business logic to service layer
- Keep controllers thin - only routing and HTTP concerns

---

#### 14. Comments in Code
**Current State**: 
- Hungarian comments in some places
- Some commented-out code
- Lack of XML documentation comments

**Recommendations**:

a) **Remove Hungarian Comments**: 
- Lines 49, 92, 119 in `UserRepository.cs`

b) **Add XML Documentation for Public APIs**:
```csharp
/// <summary>
/// Registers a new user in the system.
/// </summary>
/// <param name="request">Registration details including email and password.</param>
/// <returns>Created user details.</returns>
/// <response code="201">User successfully created.</response>
/// <response code="409">User with this email already exists.</response>
[HttpPost("register")]
[ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
```

c) **Remove Commented Code**: Clean up any dead code.

---

### Configuration & Environment

#### 15. Environment-Specific Configuration
**Current State**: Basic configuration present but not optimized.

**Recommendations**:

a) **Separate appsettings per environment**:
- `appsettings.Development.json` - development settings
- `appsettings.Production.json` - production settings  
- `appsettings.Staging.json` - staging settings

b) **Use Configuration Sections**:
```csharp
public class JwtSettings
{
    public string Authority { get; set; }
    public string Audience { get; set; }
}

// In Program.cs
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));
```

c) **Validate Configuration on Startup**:
```csharp
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (string.IsNullOrEmpty(jwtSettings?.Authority))
    throw new InvalidOperationException("JWT Authority is not configured");
```

---

#### 16. RequireHttpsMetadata Setting
**Current State**: `RequireHttpsMetadata = false` in Program.cs line 60

**Issue**: This is a security risk in production.

**Recommendation**:
```csharp
options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
```

---

### API Documentation

#### 17. Swagger/OpenAPI Configuration
**Current State**: Basic Swagger setup exists.

**Recommendations**:

a) **Enhanced Swagger Configuration**:
```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Toyzumi Webshop API",
        Version = "v1",
        Description = "API for Toyzumi e-commerce platform"
    });
    
    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
```

b) **Enable XML Documentation**:
In `kz-webshop-be.csproj`:
```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

---

### Testing

#### 18. No Test Projects
**Current State**: No unit tests or integration tests found.

**Recommendation**:

a) **Add Test Projects**:
```bash
dotnet new xunit -n kz-webshop-be.Tests
dotnet new xunit -n kz-webshop-be.IntegrationTests
```

b) **Test Critical Functionality**:
- User registration and authentication
- Product search and filtering
- Shopping cart operations
- Order processing
- Email sending (with mocked SMTP)

c) **Use Proper Testing Libraries**:
- xUnit or NUnit for test framework
- Moq for mocking dependencies
- FluentAssertions for readable assertions
- Microsoft.AspNetCore.Mvc.Testing for integration tests

**Example Test**:
```csharp
public class UserRepositoryTests
{
    [Fact]
    public async Task GetUserByEmailAsync_ExistingEmail_ReturnsUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
            
        using var context = new ApplicationDbContext(options);
        var repository = new UserRepository(context, mapper);
        
        // Act
        var user = await repository.GetUserByEmailAsync("test@example.com");
        
        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be("test@example.com");
    }
}
```

---

### DevOps & Deployment

#### 19. .gitignore Incomplete
**Current State**: Very minimal .gitignore file

**Recommendation**:
Add comprehensive .gitignore for .NET projects:
```gitignore
## Build results
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Bb]in/
[Oo]bj/

## Visual Studio files
.vs/
*.user
*.userosscache
*.suo

## User-specific files
*.rsuser
*.suo
*.user
*.userosscache
*.sln.docstates

## Secrets
appsettings.Development.json
appsettings.Production.json
*secrets.json

## NuGet
*.nupkg
packages/
```

---

#### 20. Health Check Endpoints
**Current State**: No health check endpoints.

**Recommendation**:
```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// After app.Build()
app.MapHealthChecks("/health");
```

---

### Model & Data

#### 21. DateTime Handling
**Current State**: Using `DateTime.UtcNow` in models (good!)
**Status**: ✅ Good practice

Keep using UTC for all timestamps to avoid timezone issues.

---

#### 22. Soft Delete Pattern
**Current State**: `IsDeleted` flag exists in User and Product models.

**Recommendations**:

a) **Add Global Query Filter**:
```csharp
// In ApplicationDbContext.OnModelCreating
modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
```

This automatically filters out soft-deleted records from all queries.

b) **Add DeletedAt Timestamp**:
```csharp
public DateTime? DeletedAt { get; set; }
```

---

#### 23. Missing CreatedBy/UpdatedBy Tracking
**Current State**: These fields exist but aren't automatically populated.

**Recommendation**:
Override `SaveChangesAsync` to automatically set audit fields:
```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        
    foreach (var entry in entries)
    {
        if (entry.Entity is Product product)
        {
            if (entry.State == EntityState.Added)
            {
                product.CreatedAt = DateTime.UtcNow;
                product.CreatedBy = _currentUserService.GetUserId();
            }
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = _currentUserService.GetUserId();
        }
    }
    
    return await base.SaveChangesAsync(cancellationToken);
}
```

---

### Background Jobs

#### 24. ProductActivationJob
**Current State**: Job exists and is configured with Quartz.
**Status**: ✅ Good implementation

**Minor Recommendation**:
Add logging to track when products are activated:
```csharp
public class ProductActivationJob : IJob
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductActivationJob> _logger;
    
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting ProductActivationJob");
        
        var funkosToActivate = await _db.FunkoPops
            .Where(f => !f.IsActive && f.Stock > 0 && !f.IsPreorder)
            .ToListAsync();
            
        funkosToActivate.ForEach(f => f.IsActive = true);
        
        _logger.LogInformation("Activated {Count} Funko Pops", funkosToActivate.Count);
        
        // ... similar for labubus
        
        if (funkosToActivate.Count > 0 || labubusToActivate.Count > 0)
        {
            await _db.SaveChangesAsync();
            _logger.LogInformation("ProductActivationJob completed successfully");
        }
    }
}
```

---

## 📊 SUMMARY

### Priority Levels

**🔴 Critical (Fixed)**
1. ✅ Hardcoded email credentials → Moved to configuration

**🟡 High Priority (Recommended)**
1. Implement proper Service Layer
2. Add comprehensive error handling and logging
3. Apply authorization attributes to controllers
4. Move connection strings to secrets
5. Add input validation

**🟢 Medium Priority**
1. Complete repository pattern with Unit of Work
2. Add database indexes for performance
3. Implement health checks
4. Add XML documentation
5. Split large controllers

**🔵 Low Priority (Nice to Have)**
1. Add test projects
2. Use constants instead of magic values
3. Enhance Swagger documentation
4. Improve .gitignore
5. Remove Hungarian comments

---

## 🎯 NEXT STEPS

### For Development Team:

1. **Immediate Actions**:
   - Configure email credentials using user secrets
   - Review and apply authorization attributes
   - Move sensitive configuration to secrets

2. **Short Term (1-2 weeks)**:
   - Implement service layer
   - Add structured logging with Serilog
   - Implement global exception handling
   - Add input validation

3. **Medium Term (1 month)**:
   - Add unit and integration tests
   - Implement Unit of Work pattern
   - Add health check endpoints
   - Enhance API documentation

4. **Long Term (2-3 months)**:
   - Performance optimization (indexes, caching)
   - Comprehensive test coverage
   - DevOps improvements (CI/CD)
   - Monitoring and alerting

---

## 📚 USEFUL RESOURCES

- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [Entity Framework Core Best Practices](https://docs.microsoft.com/en-us/ef/core/performance/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

---

*Generated: 2026-02-01*
*Reviewer: GitHub Copilot Code Review Agent*
