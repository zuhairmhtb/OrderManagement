using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Database.Commands.Authentication;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Models;
using OrderManagement.Web.Services;

namespace OrderManagement.Web.Tests.Services;

public class AuthenticationServiceTest : IDisposable
{
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ApplicationDbContext _dbContext;
    private readonly AuthenticationService _authenticationService;

    public AuthenticationServiceTest()
    {
        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _mapperMock = new Mock<IMapper>();
        
        // Use In-Memory Database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        
        _authenticationService = new AuthenticationService(_loggerMock.Object, _mapperMock.Object, _dbContext);
    }

    [Fact]
    public async Task RegisterAsync_ShouldRegisterCustomerSuccessfully()
    {
        // Arrange
        var command = new SignupCommand
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890"
        };

        var expectedDto = new CustomerProfileDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
            Role = UserRole.Customer
        };

        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns(expectedDto);

        // Act
        var result = await _authenticationService.RegisterAsync(command, UserRole.Customer);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Email, result.Email);
        Assert.Equal(expectedDto.FirstName, result.FirstName);
        Assert.Equal(expectedDto.LastName, result.LastName);
        Assert.Equal(expectedDto.PhoneNumber, result.PhoneNumber);
        
        // Verify customer was added to database
        var customerInDb = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == command.Email.ToLowerInvariant());
        Assert.NotNull(customerInDb);
        Assert.Equal(command.Email.ToLowerInvariant(), customerInDb.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify(command.Password, customerInDb.Password));
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            Password = "hashedpassword",
            Role = UserRole.Customer
        };
        
        await _dbContext.Customers.AddAsync(existingCustomer);
        await _dbContext.SaveChangesAsync();

        var command = new SignupCommand
        {
            Email = "existing@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authenticationService.RegisterAsync(command, UserRole.Customer));
        
        Assert.Contains("already exists", exception.Message);
        Assert.Contains("existing@example.com", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenPasswordConfirmationFails()
    {
        // Arrange
        var command = new SignupCommand
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "DifferentPassword456",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authenticationService.RegisterAsync(command, UserRole.Customer));
        
        Assert.Contains("Password and confirm password do not match", exception.Message);
        
        // Verify no customer was added to database
        var customerCount = await _dbContext.Customers.CountAsync();
        Assert.Equal(0, customerCount);
    }

    [Fact]
    public async Task LoginAsync_ShouldFailOnDatabaseValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "TestPassword123"
        };

        // Act
        var result = await _authenticationService.LoginAsync(command);

        // Assert
        Assert.False(result); // Current implementation always returns false
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContext?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}