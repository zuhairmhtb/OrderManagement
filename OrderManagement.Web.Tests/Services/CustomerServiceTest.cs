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
    private readonly ApplicationDbContext _dbContext;
    private readonly CustomerService _customerService;

    public CustomerServiceTest()
    {
        _loggerMock = new Mock<ILogger<CustomerService>>();
        _mapperMock = new Mock<IMapper>();
        
        // Use In-Memory Database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        
        _customerService = new CustomerService(_loggerMock.Object, _mapperMock.Object, _dbContext);
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

        await _dbContext.Customers.AddAsync(new Customer
        {
            Id = command.CustomerId,
            Email = "customer@example.com",
            Password = "hashedpassword",
            Role = UserRole.Customer,
            FirstName = "Test",
            LastName = "Customer",
            PhoneNumber = "+1234567890"
        });
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<Address>(It.IsAny<AddAddressCommand>()))
            .Returns((AddAddressCommand cmd) => new Address
            {
                Id = Guid.NewGuid(),
                CustomerId = cmd.CustomerId,
                Street = cmd.Street,
                City = cmd.City,
                PostalCode = cmd.PostalCode,
                Country = cmd.Country,
                State = cmd.State
            });

        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns((Customer c) => new CustomerProfileDto
            {
                Id = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                PhoneNumber = c.PhoneNumber,
                Role = c.Role,
                Addresses = c.Addresses.Select(a => new AddressDto
                {
                    Id = a.Id,
                    Street = a.Street,
                    City = a.City,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    State = a.State
                }).ToList()
            });

        // Act
        var result = await _customerService.AddAddressAsync(command);

        // Assert
        Assert.Equal(command.PostalCode, result.Addresses.First().PostalCode);
        Assert.NotNull(_dbContext.CustomerAddresses.FirstOrDefault(a => a.CustomerId == command.CustomerId && a.Id == result.Addresses.First().Id));
    }

    [Fact]
    public async Task RemoveAddressAsync_ShouldPublishRemoveAddressCommand()
    {
        // Arrange
        var command = new RemoveAddressCommand
        {
            AddressId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid() 
        };

        var address = new Address
        {
            Id = command.AddressId,
            CustomerId = command.CustomerId,
            Street = "123 Main Street",
            City = "Test City",
            PostalCode = "12345",
            Country = "USA",
            State = "Test State"
        };
        var customer = new Customer
        {
            Id = command.CustomerId,
            Email = "customer@example.com",
            Password = "hashedpassword",
            Role = UserRole.Customer,
            FirstName = "Test",
            LastName = "Customer",
            PhoneNumber = "+1234567890",
            Addresses = new List<Address> { address }
        };

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.CustomerAddresses.AddAsync(address);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns((Customer c) => new CustomerProfileDto
            {
                Id = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                PhoneNumber = c.PhoneNumber,
                Role = c.Role,
                Addresses = new List<AddressDto>() // Simulate address removal by returning empty list
            });

        // Act
        var result = await _customerService.RemoveAddressAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(_dbContext.CustomerAddresses
        .Where(a => a.Id == command.AddressId && a.CustomerId == command.CustomerId)
        .ToList()
        );
    }

    [Fact]
    public async Task UpdateAddressAsync_ShouldPublishUpdateAddressCommand()
    {
        // Arrange
        var command = new UpdateAddressCommand
        {
            AddressId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Street = "456 Updated Street",
            City = "Updated City",
            PostalCode = "54321",
            Country = "Canada",
            State = "Updated State"
        };

        var oldAddress = new Address
        {
            Id = command.AddressId,
            CustomerId = command.CustomerId,
            Street = "123 Main Street",
            City = "Test City",
            PostalCode = "12345",
            Country = "USA",
            State = "Test State"
        };
        var customer = new Customer
        {
            Id = oldAddress.CustomerId,
            Email = "customer@example.com",
            Password = "hashedpassword",
            Role = UserRole.Customer,
            FirstName = "Test",
            LastName = "Customer",
            PhoneNumber = "+1234567890",
            Addresses = new List<Address> { oldAddress }
        };

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.CustomerAddresses.AddAsync(oldAddress);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map(command, It.IsAny<Address>()))
            .Callback((UpdateAddressCommand cmd, Address addr) =>
            {
                addr.Street = cmd.Street ?? addr.Street;
                addr.City = cmd.City ?? addr.City;
                addr.PostalCode = cmd.PostalCode ?? addr.PostalCode;
                addr.Country = cmd.Country ?? addr.Country;
                addr.State = cmd.State ?? addr.State;
            });
        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns((Customer c) => new CustomerProfileDto
            {
                Id = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                PhoneNumber = c.PhoneNumber,
                Role = c.Role,
                Addresses = c.Addresses.Select(a => new AddressDto
                {
                    Id = a.Id,
                    Street = command.Street,
                    City = command.City,
                    PostalCode = command.PostalCode,
                    Country = command.Country,
                    State = command.State
                }).ToList()
            });

        // Act
        var result = await _customerService.UpdateAddressAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Street, result.Addresses.First().Street);
        Assert.Equal(command.City, result.Addresses.First().City);
        Assert.Equal(command.City, _dbContext.CustomerAddresses.First().City);
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

        var oldCustomer = new Customer
        {
            Id = command.CustomerId,
            Email = "customer@example.com",
            Password = "hashedpassword",
            Role = UserRole.Customer,
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890"
        };

        await _dbContext.Customers.AddAsync(oldCustomer);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map(
            It.IsAny<UpdateProfileCommand>(),
            It.IsAny<Customer>()
        )).Returns((UpdateProfileCommand cmd, Customer cust) =>
        {
            cust.FirstName = cmd.FirstName ?? cust.FirstName;
            cust.LastName = cmd.LastName ?? cust.LastName;
            cust.PhoneNumber = cmd.PhoneNumber ?? cust.PhoneNumber;
            return cust;
        });
        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns((Customer c) => new CustomerProfileDto
            {
                Id = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                PhoneNumber = c.PhoneNumber,
                Role = c.Role
            });

        // Act
        var result = await _customerService.UpdateProfileAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.FirstName, result.FirstName);
        Assert.Equal(command.FirstName, _dbContext.Customers.First().FirstName);
        Assert.Equal(command.LastName, result.LastName);
        Assert.Equal(command.LastName, _dbContext.Customers.First().LastName);
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
