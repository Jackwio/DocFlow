# DocFlow - Code Quality and Standards Documentation

This document outlines the comprehensive standards for code quality, testing, user experience consistency, and performance requirements for the DocFlow project.

## Table of Contents
- [Code Quality Standards](#code-quality-standards)
- [Testing Standards](#testing-standards)
- [User Experience Consistency](#user-experience-consistency)
- [Performance Requirements](#performance-requirements)

---

## Code Quality Standards

### 1. Architecture Principles

#### Clean Architecture
DocFlow follows Clean Architecture principles to ensure maintainability, scalability, and separation of concerns.

**Layer Structure:**
- **Domain Layer** (`DocFlow.Domain`): Core business logic, entities, value objects, domain events
  - No dependencies on other layers
  - Contains aggregates, entities, value objects, and domain events
  - Custom repository interfaces (if needed)

- **Domain Shared Layer** (`DocFlow.Domain.Shared`): Shared domain types
  - Enums and shared constants
  - Located in `DocFlow.Domain.Shared/Enums` folder

- **Application Layer** (`DocFlow.Application`): Use cases, commands, queries
  - Depends only on Domain layer
  - Application services must end with `ApplicationService`
  - Must be placed in folders ending with `Management`
  - Example: `DocFlow.Application/DocumentManagement/DocumentApplicationService.cs`

- **Application Contracts Layer** (`DocFlow.Application.Contracts`): DTOs and service interfaces
  - Data Transfer Objects (DTOs) in `Dtos` folder
  - Application service interfaces

- **Infrastructure Layer** (`DocFlow.EntityFrameworkCore`): External service implementations
  - Depends on Application and Domain layers
  - Custom repository implementations (e.g., `EfCoreDocumentRepository`)
  - DbContext for database access

- **Presentation Layer** (`DocFlow.HttpApi.Host`): API endpoints
  - Depends only on Infrastructure layer
  - Controllers in `Controllers` folder
  - Request/response mapping

**Dependency Rules:**
- Domain: No dependencies
- Application: Depends only on Domain
- Infrastructure: Depends on Application and Domain
- API: Depends only on Infrastructure
- No circular dependencies allowed

#### Domain-Driven Design (DDD)

**Aggregates:**
- Represent consistency boundaries and map 1:1 to business concepts
- Must be created via static factory methods (e.g., `Order.Create(...)`)
- Use private/protected constructors to enforce factory method usage
- Do not inherit from base classes (inheritance only when justified by business need)
- Can contain entities or value objects, but never other aggregates
- Responsible for enforcing all business rules and invariants

**Entities:**
- Owned by aggregates and should not be used outside of them
- Created via aggregate root factory methods
- Have unique identifiers and encapsulate state (no public setters)
- Use private/protected constructors
- Lifecycle managed by aggregate root

**Value Objects:**
- Immutable and defined only by their attributes
- No identity
- Created via constructors or static factory methods
- Must implement value equality (override Equals and GetHashCode)
- Example: `Address`, `Money`

**Strongly Typed IDs:**
- Use strongly typed IDs instead of primitive types (e.g., `OrderId`, `CustomerId`)
- Implemented as value objects
- Increases type safety and prevents confusion

**Repositories:**
- Interfaces defined in Domain or Application layer
- Implemented only for aggregate roots
- Use business-oriented method names (e.g., `FindOrderByNumber`, `PlaceOrder`)
- Avoid CRUD-style method names (no Set, Create, Update, Delete, Get)
- Implementations belong in Infrastructure layer

**Method Naming:**
- Use business-oriented, intention-revealing names
- Examples: `PlaceOrder`, `ActivateAccount`, `MarkAsShipped`
- Avoid generic CRUD verbs

### 2. Coding Style (C#)

**Naming Conventions:**
- `PascalCase` for classes, methods, and properties
- `camelCase` for local variables and method parameters
- `ALL_CAPS` for constants
- Prefix interfaces with `I` (e.g., `IOrderService`)
- Use meaningful, descriptive names; avoid abbreviations

**Formatting:**
- Use 4 spaces for indentation (no tabs)
- Use file-scoped namespaces
- One type per file
- Place opening braces on new lines for methods and types
- Add blank line between method definitions

**Modern C# Features:**
- Use `var` for local variables when type is obvious
- Use pattern matching and expression-bodied members
- Prefer object and collection initializers
- Use `nameof` for parameter names in exceptions

**Example - File-Scoped Namespaces:**
```csharp
namespace DocFlow.Domain.Documents;

public sealed class Document
{
    // ...existing code...
}
```

**Sealed Classes:**
- Make classes `sealed` by default
- Use `virtual` explicitly if inheritance is required

**Code Structure:**
- Group using directives at top of file, outside namespace
- Organize files by feature/domain
- Use partial classes only when necessary

### 3. Object Calisthenics (for Domain Code)

These rules apply **primarily to business domain code** (aggregates, entities, value objects, domain services) and **secondarily to application layer** services.

**Exemptions:** DTOs, API models/contracts, configuration classes, infrastructure code

**9 Core Principles:**

1. **One Level of Indentation per Method**
   - Keep methods simple with single level of indentation
   - Extract helper methods when needed

2. **Don't Use the ELSE Keyword**
   - Use early returns instead of else
   - Apply Fail Fast principle with guard clauses
   ```csharp
   // Good
   public void ProcessOrder(Order order)
   {
       if (order == null) throw new ArgumentNullException(nameof(order));
       if (!order.IsValid) throw new InvalidOperationException("Invalid order");
       // Process order
   }
   ```

3. **Wrap All Primitives and Strings**
   - Wrap primitives in classes for meaningful context
   - Example: Create `Age` class instead of using `int`

4. **First Class Collections**
   - Classes with collections should not have other attributes
   - Encapsulate collection behavior in dedicated classes

5. **One Dot per Line**
   - Limit method chaining to improve readability
   - Break complex chains into separate statements

6. **Don't Abbreviate**
   - Use full, meaningful names
   - Avoid confusing abbreviations

7. **Keep Entities Small**
   - Maximum 10 methods per class
   - Maximum 50 lines per class
   - Maximum 10 classes per namespace/package

8. **No Classes with More Than Two Instance Variables**
   - Encourage single responsibility
   - Loggers (e.g., `ILogger`) don't count toward the limit

9. **No Getters/Setters in Domain Classes**
   - Use private constructors and static factory methods
   - **Exception:** DTOs may have public getters/setters

### 4. Commit Conventions

Follow [Conventional Commits](https://www.conventionalcommits.org/) specification:

**Format:**
```
<type>[optional scope]: <description>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `build`: Build system changes
- `ci`: CI/CD changes
- `chore`: Maintenance tasks

**Examples:**
- `feat(api): add order endpoint`
- `fix(domain): correct order validation logic`
- `test(order): add unit tests for order creation`
- `docs: update code quality standards`

**Branch Naming:**
```
<type>/<short-description-with-hyphens>
```
Examples: `feat/add-user-login`, `fix/order-calculation`, `docs/update-readme`

---

## Testing Standards

### 1. Test Framework and Tools

**Required Libraries:**
- **Test Framework**: xUnit v3 (for all tests)
- **Mocking**: FakeItEasy (for unit tests)
- **Integration Testing**: Testcontainers (for databases and external dependencies)
- **Contract Testing**: Microcks (for SOAP/REST simulations)

### 2. Test Organization

**Unit Tests:**
- **Location**: `tests/DocFlow.UnitTests/`
- **Scope**: Test Domain and Application layers only
- **Purpose**: Validate business logic, use cases, and validation
- **Dependencies**: Mock all dependencies using FakeItEasy
- **No Real Services**: No database or external service interactions
- **Folders**: Organize by use cases and services

**Integration Tests:**
- **Location**: `tests/DocFlow.IntegrationTests/`
- **Scope**: Test Infrastructure and API layers, and cross-layer integration
- **Purpose**: Validate entire business flows, data persistence, external calls
- **Dependencies**: Use Testcontainers for databases and external services
- **Contract Tests**: Use Microcks for SOAP/REST/event simulations
- **Folders**: Organize by features (e.g., `Features/` for API endpoint tests)

**Architecture Tests:**
- **Location**: `tests/DocFlow.IntegrationTests/ArchitectureTests.cs`
- **Purpose**: Enforce layer dependencies and architectural rules
- **Tool**: NetArchTest
- **Validations**:
  - Enforce allowed/forbidden dependencies between layers
  - Check for forbidden dependencies (e.g., EntityFrameworkCore in API/Domain)
  - Optional: Check domain immutability

### 3. Test-Driven Development (TDD)

**Process:**
1. Write tests before implementation
2. Run tests to verify they fail (Red)
3. Implement minimal code to pass tests (Green)
4. Refactor while keeping tests passing (Refactor)

**Best Practices:**
- Write tests for both valid and invalid scenarios
- Use descriptive test names that explain the purpose
- Verify exceptions and expected results
- Document test cases in test files
- Run `dotnet test` after every significant change

### 4. Test Examples

**Unit Test Example:**
```csharp
using Xunit;
using FakeItEasy;

namespace DocFlow.Application.Tests;

public class DocumentApplicationServiceTests
{
    [Fact]
    public void CreateDocument_WithValidData_ShouldSucceed()
    {
        // Arrange
        var repository = A.Fake<IDocumentRepository>();
        var service = new DocumentApplicationService(repository);
        
        // Act
        var result = service.CreateDocument(name: "Test Doc", content: "Content");
        
        // Assert
        Assert.NotNull(result);
        A.CallTo(() => repository.Insert(A<Document>._)).MustHaveHappened();
    }
}
```

**Integration Test Example:**
```csharp
using Xunit;
using Testcontainers.PostgreSql;

namespace DocFlow.IntegrationTests.Features;

public class DocumentApiTests : IAsyncLifetime
{
    private PostgreSqlContainer _dbContainer;
    
    public async Task InitializeAsync()
    {
        _dbContainer = new PostgreSqlBuilder().Build();
        await _dbContainer.StartAsync();
    }
    
    [Fact]
    public async Task CreateDocument_ShouldPersistToDatabase()
    {
        // Arrange & Act & Assert
        // Test implementation with real database
    }
    
    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
```

### 5. Continuous Testing

- Run `dotnet test` after every significant change
- Use `dotnet watch test` for continuous test execution during development
- All tests must pass before committing code
- Document test results in task/user story

---

## User Experience Consistency

### 1. UI/UX Standards

**Blazor Components:**
- Follow consistent component structure across the application
- Use ABP Framework's built-in Blazor components for consistency
- Maintain consistent styling and theming

**API Consistency:**
- RESTful API design principles
- Consistent error response formats
- Standard HTTP status codes
- Versioned APIs when breaking changes occur

**Naming Consistency:**
- Use consistent terminology across UI, API, and domain
- Follow ubiquitous language from DDD
- Maintain consistent field labels and messages

### 2. Accessibility

- Follow WCAG 2.1 AA guidelines
- Ensure keyboard navigation support
- Provide appropriate ARIA labels
- Support screen readers

### 3. Error Handling

**User-Facing Errors:**
- Provide clear, actionable error messages
- Avoid technical jargon in user-facing messages
- Include guidance on how to resolve errors

**API Error Responses:**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input provided",
    "details": [
      {
        "field": "documentName",
        "message": "Document name is required"
      }
    ]
  }
}
```

### 4. Internationalization (i18n)

- Support multiple languages using ABP's localization system
- Store all user-facing text in resource files
- Format dates, numbers, and currencies according to locale

### 5. Responsive Design

- Support desktop, tablet, and mobile viewports
- Use responsive Blazor components
- Test on multiple screen sizes
- Optimize for touch and mouse interactions

---

## Performance Requirements

### 1. API Response Time

**Targets:**
- **Simple queries** (e.g., get by ID): < 100ms (p95)
- **List operations** (paginated): < 300ms (p95)
- **Complex operations** (with joins): < 500ms (p95)
- **Write operations**: < 200ms (p95)

**Monitoring:**
- Use Application Insights or similar APM tools
- Track p50, p95, and p99 response times
- Set up alerts for performance degradation

### 2. Database Performance

**Query Optimization:**
- Use appropriate indexes on frequently queried columns
- Avoid N+1 query problems
- Use `Include()` for eager loading when needed
- Implement pagination for list queries (max 100 items per page)

**Connection Pooling:**
- Use Entity Framework Core's built-in connection pooling
- Configure appropriate pool size based on load

**Caching Strategy:**
- Cache frequently accessed, rarely changing data
- Use distributed caching (Redis) for multi-instance deployments
- Implement cache invalidation strategies

### 3. Scalability

**Horizontal Scaling:**
- Design stateless APIs to support multiple instances
- Use distributed caching for session state
- Implement queue-based processing for long-running operations

**Resource Management:**
- Limit concurrent requests per user/client
- Implement rate limiting on public APIs
- Use async/await for I/O-bound operations

### 4. Asset Optimization

**Frontend:**
- Minify JavaScript and CSS
- Optimize images (compress, use appropriate formats)
- Implement lazy loading for components
- Use CDN for static assets

**Backend:**
- Enable response compression (gzip/brotli)
- Optimize serialization (use System.Text.Json)
- Minimize payload sizes

### 5. Performance Testing

**Load Testing:**
- Test under expected load conditions
- Identify bottlenecks before production
- Use tools like k6, JMeter, or Apache Bench

**Metrics to Track:**
- Requests per second (RPS)
- Response time percentiles (p50, p95, p99)
- Error rate
- Database query times
- Memory usage
- CPU utilization

**Performance Benchmarks:**
```csharp
// Example using BenchmarkDotNet
[Benchmark]
public async Task<Document> GetDocument()
{
    return await _repository.GetAsync(id);
}
```

### 6. Database Connection Management

- Use `using` statements or `await using` for proper disposal
- Implement retry policies for transient failures
- Monitor connection pool exhaustion
- Set appropriate command timeouts

### 7. Memory Management

**Guidelines:**
- Dispose of unmanaged resources properly
- Avoid memory leaks in long-running operations
- Use memory profiling tools to identify issues
- Implement streaming for large file operations

---

## Enforcement and Validation

### 1. Code Review Checklist

Before approving any PR, verify:
- [ ] Follows Clean Architecture principles
- [ ] Adheres to DDD guidelines for domain code
- [ ] Follows Object Calisthenics for domain/application code
- [ ] Uses proper C# coding style
- [ ] Has appropriate unit and/or integration tests
- [ ] Passes all existing tests
- [ ] Follows Conventional Commits format
- [ ] Includes performance considerations
- [ ] Maintains UX consistency

### 2. Automated Checks

**Build Pipeline:**
- Compile all projects without warnings
- Run all unit and integration tests
- Run architecture validation tests
- Check code coverage (target: >80% for domain/application layers)
- Run static code analysis tools

**Pre-commit Hooks (Recommended):**
- Format code with `dotnet format`
- Run unit tests
- Validate commit message format

### 3. Documentation Updates

- Update this document when introducing new standards
- Document architectural decisions in ADRs (Architecture Decision Records)
- Keep README.md current with project setup instructions

---

## References and Resources

### Official Documentation
- [ABP Framework Documentation](https://abp.io/docs)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/TheCleanArchitecture.html)
- [Domain-Driven Design](https://www.domainlanguage.com/ddd/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Microsoft .NET C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

### Books
- "Domain-Driven Design: Tackling Complexity in the Heart of Software" by Eric Evans
- "Implementing Domain-Driven Design" by Vaughn Vernon
- "Clean Code: A Handbook of Agile Software Craftsmanship" by Robert C. Martin
- "Clean Architecture: A Craftsman's Guide to Software Structure and Design" by Robert C. Martin

### Tools
- [xUnit](https://xunit.net/) - Testing framework
- [FakeItEasy](https://fakeiteasy.github.io/) - Mocking library
- [Testcontainers](https://dotnet.testcontainers.org/) - Integration testing
- [NetArchTest](https://github.com/BenMorris/NetArchTest) - Architecture testing
- [BenchmarkDotNet](https://benchmarkdotnet.org/) - Performance benchmarking

---

## Continuous Improvement

This document should be treated as a living document and updated as:
- New best practices are discovered
- Tools and frameworks are upgraded
- Team learns from retrospectives
- Performance requirements change

Last Updated: 2025-11-09
Version: 1.0
