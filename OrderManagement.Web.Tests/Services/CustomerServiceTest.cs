using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Models;
using OrderManagement.Web.Services;

namespace OrderManagement.Web.Tests.Services;

public class CustomerServiceTest : IDisposable
{
    private readonly Mock<ILogger<CustomerService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ApplicationDbContext _dbContext;
    private readonly CustomerService _customerService;

    public CustomerServiceTest()
    {
        _loggerMock = new Mock<ILogger<CustomerService>>();
        _mapperMock = new Mock<IMapper>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        
        // Use In-Memory Database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        
        _customerService = new CustomerService(_loggerMock.Object, _mapperMock.Object, _publishEndpointMock.Object, _dbContext);
    }

    [Fact]
    public async Task GetCustomerProfileAsync_ShouldReturnData()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Email = "test@example.com",
            Password = "hashedpassword",
            Role = UserRole.Customer,
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890"
        };

        var expectedDto = new CustomerProfileDto
        {
            Id = customerId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
            Role = UserRole.Customer
        };

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns(expectedDto);

        // Act
        var result = await _customerService.GetCustomerProfileAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Email, result.Email);
        Assert.Equal(expectedDto.FirstName, result.FirstName);
        Assert.Equal(expectedDto.LastName, result.LastName);
        Assert.Equal(expectedDto.PhoneNumber, result.PhoneNumber);
        
        _mapperMock.Verify(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomerProfileAsync_ShouldReturnError_OnDbValidationError()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _customerService.GetCustomerProfileAsync(nonExistentCustomerId));
        
        Assert.Contains("not found", exception.Message);
        Assert.Contains(nonExistentCustomerId.ToString(), exception.Message);
        
        _mapperMock.Verify(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task SearchCustomersAsync_ShouldReturnData_WithEmailFilter()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                Email = "john@example.com",
                Password = "hash1",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Customer
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Email = "jane@example.com",
                Password = "hash2",
                FirstName = "Jane",
                LastName = "Smith",
                Role = UserRole.Customer
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Email = "bob@test.com",
                Password = "hash3",
                FirstName = "Bob",
                LastName = "Johnson",
                Role = UserRole.Customer
            }
        };

        await _dbContext.Customers.AddRangeAsync(customers);
        await _dbContext.SaveChangesAsync();

        var expectedDtos = customers.Where(c => c.Email.Contains("example"))
            .Select(c => new CustomerProfileDto
            {
                Id = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Role = c.Role
            }).ToList();

        _mapperMock.Setup(m => m.Map<IEnumerable<CustomerProfileDto>>(It.IsAny<List<Customer>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _customerService.SearchCustomersAsync(emailPattern: "example");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, dto => Assert.Contains("example", dto.Email));
        
        _mapperMock.Verify(m => m.Map<IEnumerable<CustomerProfileDto>>(It.IsAny<List<Customer>>()), Times.Once);
    }

    [Fact]
    public async Task SearchCustomersAsync_ShouldReturnData_WithNameFilter()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                Email = "john@example.com",
                Password = "hash1",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Customer
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Email = "jane@example.com",
                Password = "hash2",
                FirstName = "Jane",
                LastName = "Smith",
                Role = UserRole.Customer
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Email = "bob@test.com",
                Password = "hash3",
                FirstName = "Bob",
                LastName = "Johnson",
                Role = UserRole.Customer
            }
        };

        await _dbContext.Customers.AddRangeAsync(customers);
        await _dbContext.SaveChangesAsync();

        var expectedDtos = customers.Where(c => (c.FirstName?.Contains("Jo") == true) || (c.LastName?.Contains("Jo") == true))
            .Select(c => new CustomerProfileDto
            {
                Id = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Role = c.Role
            }).ToList();

        _mapperMock.Setup(m => m.Map<IEnumerable<CustomerProfileDto>>(It.IsAny<List<Customer>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _customerService.SearchCustomersAsync(namePattern: "Jo");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, dto => dto.FirstName == "John");
        Assert.Contains(result, dto => dto.LastName == "Johnson");
        
        _mapperMock.Verify(m => m.Map<IEnumerable<CustomerProfileDto>>(It.IsAny<List<Customer>>()), Times.Once);
    }

    [Fact]
    public async Task SearchCustomersAsync_ShouldReturnData_WithPagination()
    {
        // Arrange
        var customers = new List<Customer>();
        for (int i = 1; i <= 25; i++)
        {
            customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                Email = $"user{i:D2}@example.com",
                Password = $"hash{i}",
                FirstName = $"User{i}",
                LastName = "Test",
                Role = UserRole.Customer
            });
        }

        await _dbContext.Customers.AddRangeAsync(customers);
        await _dbContext.SaveChangesAsync();

        var expectedDtos = customers.OrderBy(c => c.Email)
            .Skip(20) // Page 2, pageSize 20
            .Take(20)
            .Select(c => new CustomerProfileDto
            {
                Id = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Role = c.Role
            }).ToList();

        _mapperMock.Setup(m => m.Map<IEnumerable<CustomerProfileDto>>(It.IsAny<List<Customer>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _customerService.SearchCustomersAsync(page: 2, pageSize: 20);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count()); // Should only have 5 items on page 2 (items 21-25)
        
        _mapperMock.Verify(m => m.Map<IEnumerable<CustomerProfileDto>>(It.IsAny<List<Customer>>()), Times.Once);
    }

    [Fact]
    public async Task SearchCustomersAsync_ShouldReturnData_WithCombinedFilters()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                Email = "john@example.com",
                Password = "hash1",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Customer
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Email = "jane@example.com",
                Password = "hash2",
                FirstName = "Jane",
                LastName = "Smith",
                Role = UserRole.Customer
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Email = "john@test.com",
                Password = "hash3",
                FirstName = "John",
                LastName = "Johnson",
                Role = UserRole.Customer
            }
        };

        await _dbContext.Customers.AddRangeAsync(customers);
        await _dbContext.SaveChangesAsync();

        var expectedDtos = customers.Where(c => c.Email.Contains("example") && (c.FirstName?.Contains("John") == true))
            .Select(c => new CustomerProfileDto
            {
                Id = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Role = c.Role
            }).ToList();

        _mapperMock.Setup(m => m.Map<IEnumerable<CustomerProfileDto>>(It.IsAny<List<Customer>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _customerService.SearchCustomersAsync(emailPattern: "example", namePattern: "John");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("john@example.com", result.First().Email);
        Assert.Equal("John", result.First().FirstName);
        
        _mapperMock.Verify(m => m.Map<IEnumerable<CustomerProfileDto>>(It.IsAny<List<Customer>>()), Times.Once);
    }

    [Fact]
    public async Task SearchCustomersAsync_ShouldReturnEmptyList_WhenNoMatches()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                Email = "john@example.com",
                Password = "hash1",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Customer
            }
        };

        await _dbContext.Customers.AddRangeAsync(customers);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<IEnumerable<CustomerProfileDto>>(It.IsAny<List<Customer>>()))
            .Returns(new List<CustomerProfileDto>());

        // Act
        var result = await _customerService.SearchCustomersAsync(emailPattern: "nonexistent");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        _mapperMock.Verify(m => m.Map<IEnumerable<CustomerProfileDto>>(It.IsAny<List<Customer>>()), Times.Once);
    }

    [Fact]
    public async Task AddAddressAsync_ShouldPublishAddAddressCommand()
    {
        // Arrange
        var command = new AddAddressCommand
        {
            CustomerId = Guid.NewGuid(),
            Street = "123 Main Street",
            City = "Test City",
            PostalCode = "12345",
            Country = "USA",
            State = "Test State"
        };

        // Act
        var result = await _customerService.AddAddressAsync(command);

        // Assert
        Assert.True(result);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<AddAddressCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAddressAsync_ShouldPublishRemoveAddressCommand()
    {
        // Arrange
        var command = new RemoveAddressCommand
        {
            AddressId = Guid.NewGuid()
        };

        // Act
        var result = await _customerService.RemoveAddressAsync(command);

        // Assert
        Assert.True(result);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<RemoveAddressCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAddressAsync_ShouldPublishUpdateAddressCommand()
    {
        // Arrange
        var command = new UpdateAddressCommand
        {
            AddressId = Guid.NewGuid(),
            Street = "456 Updated Street",
            City = "Updated City",
            PostalCode = "54321",
            Country = "Canada",
            State = "Updated State"
        };

        // Act
        var result = await _customerService.UpdateAddressAsync(command);

        // Assert
        Assert.True(result);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<UpdateAddressCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldPublishUpdateProfileCommand()
    {
        // Arrange
        var command = new UpdateProfileCommand
        {
            CustomerId = Guid.NewGuid(),
            FirstName = "Updated John",
            LastName = "Updated Doe",
            PhoneNumber = "+1987654321"
        };

        // Act
        var result = await _customerService.UpdateProfileAsync(command);

        // Assert
        Assert.True(result);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<UpdateProfileCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
