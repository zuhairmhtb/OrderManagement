using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Database.Commands.Order;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Dtos.Order;
using OrderManagement.Database.Events.Order;
using OrderManagement.Database.Models;
using OrderManagement.Web.Services;

namespace OrderManagement.Web.Tests.Services;

public class OrderServiceTest : IDisposable
{
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ApplicationDbContext _dbContext;
    private readonly OrderService _orderService;

    private readonly PlaceOrderCommand _sampleCommand;
    private readonly Customer _sampleCustomer;
    private readonly IEnumerable<Product> _sampleProducts;

    public OrderServiceTest()
    {
        _loggerMock = new Mock<ILogger<OrderService>>();
        _mapperMock = new Mock<IMapper>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _orderService = new OrderService(_loggerMock.Object, _mapperMock.Object, _publishEndpointMock.Object, _dbContext);

        _sampleCommand = new PlaceOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Products = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = 1, Quantity = 2 }
            },
            ShippingAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Test City",
                PostalCode = "12345",
                Country = "USA"
            },
            BillingAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Test City",
                PostalCode = "12345",
                Country = "USA"
            },
            Currency = Currency.USD.ToString()
        };
        _sampleCustomer = new Customer
        {
            Id = _sampleCommand.CustomerId,
            Email = "customer@example.com",
            Password = "hashedpassword",
            Role = UserRole.Customer,
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
        };
        _sampleProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Test Product", Price = 10.00, Quantity = 100 },
            new Product { Id = 2, Name = "Another Product", Price = 20.00, Quantity = 50 }
        };
    }

    [Fact]
    public async Task PlaceOrderAsync_ShouldPublishPlaceOrderCommand()
    {
        // Arrange
        _dbContext.Products.AddRange(_sampleProducts);
        _dbContext.Customers.Add(_sampleCustomer);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(x => x.Map<CustomerOrderDto>(It.IsAny<Order>())).Returns((Order o) =>
        {
            return new CustomerOrderDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                DeliveryDate = o.DeliveryDate,
                OrderStatus = o.OrderStatus.ToString(),

                CustomerEmail = o.CustomerEmail,

                Currency = o.Currency.ToString(),
                Subtotal = o.Subtotal,
                Total = o.TotalAmount,

                Items = o.Products.Select(p => new OrderItemDto
                {
                    ProductId = p.ProductId,

                    Quantity = p.Quantity
                }).ToList(),

                ShippingAddress = new AddressDto
                {
                    Street = o.ShippingStreet,
                    City = o.ShippingCity,
                    PostalCode = o.ShippingPostalCode,
                    Country = o.ShippingCountry
                },

                BillingAddress = new AddressDto
                {
                    Street = o.BillingStreet,
                    City = o.BillingCity,
                    PostalCode = o.BillingPostalCode,
                    Country = o.BillingCountry
                }

            };
        });

        // Act
        var result = await _orderService.PlaceOrderAsync(_sampleCommand);

        // Assert
        Assert.Equal(OrderStatus.Pending.ToString(), result.OrderStatus);

        var dbRecord = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == result.OrderId);
        Assert.NotNull(dbRecord);
        Assert.Equal(_sampleCommand.CustomerId, dbRecord.CustomerId);
        Assert.NotEqual(0, dbRecord.Subtotal);
        Assert.True(dbRecord.Products.Select(x => x.ProductId).All(id => _sampleCommand.Products.Any(p => p.ProductId == id)));
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<PlacedOrderEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_ShouldReturnErrorIfCustomerDoesNotExist()
    {
        // Arrange
        _dbContext.Products.AddRange(_sampleProducts);
        await _dbContext.SaveChangesAsync();

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _orderService.PlaceOrderAsync(_sampleCommand));

        // Assert
        Assert.Contains("Customer", exception.Message);
        Assert.Contains("was not found", exception.Message);
    }

    [Fact]
    public async Task PlaceOrderAsync_ShouldReturnErrorIfCustomerPhoneNumberDoesNotExist()
    {
        // Arrange
        var customerWithContact =  new Customer
        {
            Id = _sampleCommand.CustomerId,
            Email = "customer@example.com",
            Password = "hashedpassword",
            Role = UserRole.Customer,
            FirstName = "John",
            LastName = "Doe"
        };
        await _dbContext.Customers.AddAsync(customerWithContact);
        _dbContext.Products.AddRange(_sampleProducts);
        await _dbContext.SaveChangesAsync();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
        _orderService.PlaceOrderAsync(_sampleCommand));

        // Assert
        Assert.Contains("Customer", exception.Message);
        Assert.Contains("no phone number", exception.Message);
    }

    [Fact]
    public async Task PlaceOrderAsync_ShouldReturnErrorIfCurrencyDoesNotExist()
    {
        // Arrange
        await _dbContext.Customers.AddAsync(_sampleCustomer);
        _dbContext.Products.AddRange(_sampleProducts);
        await _dbContext.SaveChangesAsync();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
        _orderService.PlaceOrderAsync(new PlaceOrderCommand
        {
            CustomerId = _sampleCommand.CustomerId,
            Products = _sampleCommand.Products,
            ShippingAddress = _sampleCommand.ShippingAddress,
            BillingAddress = _sampleCommand.BillingAddress,
            Currency = "INVALID_CURRENCY"
        }));

        // Assert
        Assert.Contains("Invalid currency code", exception.Message);
    }

    [Fact]
    public async Task PlaceOrderAsync_ShouldReturnErrorIfProductIsMissing()
    {
        // Arrange
        await _dbContext.Customers.AddAsync(_sampleCustomer);
        _dbContext.Products.AddRange(new List<Product>
        {
            new Product { Id = 2, Name = "Test Product", Price = 10.00, Quantity = 100 }
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
        _orderService.PlaceOrderAsync(_sampleCommand));

        // Assert
        Assert.Contains("products were not found", exception.Message);
    }

     [Fact]
    public async Task PlaceOrderAsync_ShouldReturnErrorIfProductHasInsufficientStock()
    {
        // Arrange
        await _dbContext.Customers.AddAsync(_sampleCustomer);
        _dbContext.Products.AddRange(new List<Product>
        {
            new Product { Id = 1, Name = "Test Product", Price = 10.00, Quantity = 0 }
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
        _orderService.PlaceOrderAsync(_sampleCommand));

        // Assert
        Assert.Contains("Insufficient stock", exception.Message);
    }

    [Fact]
    public async Task GetOrderDetailsAsync_ShouldReturnData()
    {
        // Arrange
        var orderId = Guid.NewGuid();
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

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CustomerEmail = "test@example.com",
            CustomerContactNumber = "+1234567890",
            OrderStatus = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            Currency = Currency.USD,
            Subtotal = 100.00,
            TotalAmount = 110.00,
            ShippingStreet = "123 Test Street",
            ShippingCity = "Test City",
            ShippingPostalCode = "12345",
            ShippingCountry = "USA",
            BillingStreet = "123 Test Street",
            BillingCity = "Test City",
            BillingPostalCode = "12345",
            BillingCountry = "USA",
            Products = new List<PurchasedProduct>
            {
                new PurchasedProduct
                {
                    Id = Guid.NewGuid(),
                    ProductId = 1,
                    Name = "Test Product",
                    Price = 50.00,
                    Quantity = 2,
                    OrderId = orderId
                }
            }
        };

        var expectedOrderDto = new CustomerOrderDto
        {
            OrderId = orderId,
            CustomerEmail = "test@example.com",
            OrderStatus = OrderStatus.Pending.ToString(),
            OrderDate = order.OrderDate,
            Subtotal = 100.00,
            Total = 110.00
        };

        var expectedCustomerDto = new CustomerProfileDto
        {
            Id = customerId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
            Role = UserRole.Customer
        };

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<CustomerOrderDto>(It.IsAny<Order>()))
            .Returns(expectedOrderDto);
        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns(expectedCustomerDto);

        // Act
        var result = await _orderService.GetOrderDetailsAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrderDto.OrderId, result.OrderId);
        Assert.Equal(expectedOrderDto.CustomerEmail, result.CustomerEmail);
        Assert.Equal(expectedOrderDto.OrderStatus, result.OrderStatus);

        _mapperMock.Verify(m => m.Map<CustomerOrderDto>(It.IsAny<Order>()), Times.Once);
        _mapperMock.Verify(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task GetOrderDetailsAsync_ShouldReturnError_OnDbValidationError()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.GetOrderDetailsAsync(nonExistentOrderId));

        Assert.Contains("not found", exception.Message);
        Assert.Contains(nonExistentOrderId.ToString(), exception.Message);

        _mapperMock.Verify(m => m.Map<CustomerOrderDto>(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task GetOrderStatusAsync_ShouldReturnStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CustomerContactNumber = "+1234567890",
            OrderStatus = OrderStatus.Delivered,
            OrderDate = DateTime.UtcNow,
            Currency = Currency.USD,
            Subtotal = 100.00,
            TotalAmount = 110.00,
            ShippingStreet = "123 Test Street",
            ShippingCity = "Test City",
            ShippingPostalCode = "12345",
            ShippingCountry = "USA",
            BillingStreet = "123 Test Street",
            BillingCity = "Test City",
            BillingPostalCode = "12345",
            BillingCountry = "USA"
        };

        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _orderService.GetOrderStatusAsync(orderId);

        // Assert
        Assert.Equal(OrderStatus.Delivered, result);
    }

    [Fact]
    public async Task GetOrderStatusAsync_ShouldReturnError_OnDbValidationError()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.GetOrderStatusAsync(nonExistentOrderId));

        Assert.Contains("not found", exception.Message);
        Assert.Contains(nonExistentOrderId.ToString(), exception.Message);
    }

    [Fact]
    public async Task SearchOrdersAsync_ShouldReturnData_WithCustomerIdFilter()
    {
        // Arrange
        var customerId1 = Guid.NewGuid();
        var customerId2 = Guid.NewGuid();

        var customer1 = new Customer
        {
            Id = customerId1,
            Email = "customer1@example.com",
            Password = "hash1",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Customer
        };

        var customer2 = new Customer
        {
            Id = customerId2,
            Email = "customer2@example.com",
            Password = "hash2",
            FirstName = "Jane",
            LastName = "Smith",
            Role = UserRole.Customer
        };

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId1,
                CustomerEmail = "customer1@example.com",
                CustomerContactNumber = "+1234567890",
                OrderStatus = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                Currency = Currency.USD,
                Subtotal = 100.00,
                TotalAmount = 110.00,
                ShippingStreet = "123 Main St",
                ShippingCity = "New York",
                ShippingPostalCode = "10001",
                ShippingCountry = "USA",
                BillingStreet = "123 Main St",
                BillingCity = "New York",
                BillingPostalCode = "10001",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId2,
                CustomerEmail = "customer2@example.com",
                CustomerContactNumber = "+1987654321",
                OrderStatus = OrderStatus.Delivered,
                OrderDate = DateTime.UtcNow.AddDays(-1),
                Currency = Currency.USD,
                Subtotal = 200.00,
                TotalAmount = 220.00,
                ShippingStreet = "456 Oak Ave",
                ShippingCity = "Los Angeles",
                ShippingPostalCode = "90210",
                ShippingCountry = "USA",
                BillingStreet = "456 Oak Ave",
                BillingCity = "Los Angeles",
                BillingPostalCode = "90210",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId1,
                CustomerEmail = "customer1@example.com",
                CustomerContactNumber = "+1234567890",
                OrderStatus = OrderStatus.Shipped,
                OrderDate = DateTime.UtcNow.AddDays(-2),
                Currency = Currency.USD,
                Subtotal = 150.00,
                TotalAmount = 165.00,
                ShippingStreet = "123 Main St",
                ShippingCity = "New York",
                ShippingPostalCode = "10001",
                ShippingCountry = "USA",
                BillingStreet = "123 Main St",
                BillingCity = "New York",
                BillingPostalCode = "10001",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            }
        };

        await _dbContext.Customers.AddRangeAsync(new[] { customer1, customer2 });
        await _dbContext.Orders.AddRangeAsync(orders);
        await _dbContext.SaveChangesAsync();

        var expectedDtos = orders.Where(o => o.CustomerId == customerId1)
            .Select(o => new CustomerOrderDto
            {
                OrderId = o.Id,
                CustomerEmail = o.CustomerEmail,
                OrderStatus = o.OrderStatus.ToString(),
                OrderDate = o.OrderDate,
                Subtotal = o.Subtotal,
                Total = o.TotalAmount
            }).ToList();

        _mapperMock.Setup(m => m.Map<CustomerOrderDto>(It.IsAny<Order>()))
            .Returns((Order order) => new CustomerOrderDto
            {
                OrderId = order.Id,
                CustomerEmail = order.CustomerEmail,
                OrderStatus = order.OrderStatus.ToString(),
                OrderDate = order.OrderDate,
                Subtotal = order.Subtotal,
                Total = order.TotalAmount
            });

        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns((Customer customer) => new CustomerProfileDto
            {
                Id = customer.Id,
                Email = customer.Email,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Role = customer.Role
            });

        // Act
        var result = await _orderService.SearchOrdersAsync(customerId: customerId1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, dto => Assert.Equal("customer1@example.com", dto.CustomerEmail));
    }

    [Fact]
    public async Task SearchOrdersAsync_ShouldReturnData_WithEmailFilter()
    {
        // Arrange
        var customerId1 = Guid.NewGuid();
        var customerId2 = Guid.NewGuid();

        var customer1 = new Customer
        {
            Id = customerId1,
            Email = "john@example.com",
            Password = "hash1",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Customer
        };

        var customer2 = new Customer
        {
            Id = customerId2,
            Email = "jane@test.com",
            Password = "hash2",
            FirstName = "Jane",
            LastName = "Smith",
            Role = UserRole.Customer
        };

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId1,
                CustomerEmail = "john@example.com",
                CustomerContactNumber = "+1234567890",
                OrderStatus = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                Currency = Currency.USD,
                Subtotal = 100.00,
                TotalAmount = 110.00,
                ShippingStreet = "123 Main St",
                ShippingCity = "Chicago",
                ShippingPostalCode = "60601",
                ShippingCountry = "USA",
                BillingStreet = "123 Main St",
                BillingCity = "Chicago",
                BillingPostalCode = "60601",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId2,
                CustomerEmail = "jane@test.com",
                CustomerContactNumber = "+1987654321",
                OrderStatus = OrderStatus.Delivered,
                OrderDate = DateTime.UtcNow.AddDays(-1),
                Currency = Currency.USD,
                Subtotal = 200.00,
                TotalAmount = 220.00,
                ShippingStreet = "456 Pine St",
                ShippingCity = "Miami",
                ShippingPostalCode = "33101",
                ShippingCountry = "USA",
                BillingStreet = "456 Pine St",
                BillingCity = "Miami",
                BillingPostalCode = "33101",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            }
        };

        await _dbContext.Customers.AddRangeAsync(new[] { customer1, customer2 });
        await _dbContext.Orders.AddRangeAsync(orders);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<CustomerOrderDto>(It.IsAny<Order>()))
            .Returns((Order order) => new CustomerOrderDto
            {
                OrderId = order.Id,
                CustomerEmail = order.CustomerEmail,
                OrderStatus = order.OrderStatus.ToString(),
                OrderDate = order.OrderDate,
                Subtotal = order.Subtotal,
                Total = order.TotalAmount
            });

        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns((Customer customer) => new CustomerProfileDto
            {
                Id = customer.Id,
                Email = customer.Email,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Role = customer.Role
            });

        // Act
        var result = await _orderService.SearchOrdersAsync(customerEmailPattern: "example");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("example", result.First().CustomerEmail);
    }

    [Fact]
    public async Task SearchOrdersAsync_ShouldReturnData_WithStatusFilter()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            Email = "test@example.com",
            Password = "hash",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Customer
        };

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerEmail = "test@example.com",
                CustomerContactNumber = "+1234567890",
                OrderStatus = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                Currency = Currency.USD,
                Subtotal = 100.00,
                TotalAmount = 110.00,
                ShippingStreet = "789 Test Ave",
                ShippingCity = "Seattle",
                ShippingPostalCode = "98101",
                ShippingCountry = "USA",
                BillingStreet = "789 Test Ave",
                BillingCity = "Seattle",
                BillingPostalCode = "98101",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerEmail = "test@example.com",
                CustomerContactNumber = "+1234567890",
                OrderStatus = OrderStatus.Delivered,
                OrderDate = DateTime.UtcNow.AddDays(-1),
                Currency = Currency.USD,
                Subtotal = 200.00,
                TotalAmount = 220.00,
                ShippingStreet = "789 Test Ave",
                ShippingCity = "Seattle",
                ShippingPostalCode = "98101",
                ShippingCountry = "USA",
                BillingStreet = "789 Test Ave",
                BillingCity = "Seattle",
                BillingPostalCode = "98101",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            }
        };

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Orders.AddRangeAsync(orders);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<CustomerOrderDto>(It.IsAny<Order>()))
            .Returns((Order order) => new CustomerOrderDto
            {
                OrderId = order.Id,
                CustomerEmail = order.CustomerEmail,
                OrderStatus = order.OrderStatus.ToString(),
                OrderDate = order.OrderDate,
                Subtotal = order.Subtotal,
                Total = order.TotalAmount
            });

        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns((Customer customer) => new CustomerProfileDto
            {
                Id = customer.Id,
                Email = customer.Email,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Role = customer.Role
            });

        // Act
        var result = await _orderService.SearchOrdersAsync(status: OrderStatus.Delivered);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(OrderStatus.Delivered.ToString(), result.First().OrderStatus);
    }

    [Fact]
    public async Task SearchOrdersAsync_ShouldReturnData_WithDateRangeFilter()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            Email = "test@example.com",
            Password = "hash",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Customer
        };

        var baseDate = DateTime.UtcNow.Date;
        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerEmail = "test@example.com",
                CustomerContactNumber = "+1234567890",
                OrderStatus = OrderStatus.Pending,
                OrderDate = baseDate.AddDays(-10), // Outside range
                Currency = Currency.USD,
                Subtotal = 100.00,
                TotalAmount = 110.00,
                ShippingStreet = "321 Date St",
                ShippingCity = "Phoenix",
                ShippingPostalCode = "85001",
                ShippingCountry = "USA",
                BillingStreet = "321 Date St",
                BillingCity = "Phoenix",
                BillingPostalCode = "85001",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerEmail = "test@example.com",
                CustomerContactNumber = "+1234567890",
                OrderStatus = OrderStatus.Delivered,
                OrderDate = baseDate.AddDays(-3), // Within range
                Currency = Currency.USD,
                Subtotal = 200.00,
                TotalAmount = 220.00,
                ShippingStreet = "321 Date St",
                ShippingCity = "Phoenix",
                ShippingPostalCode = "85001",
                ShippingCountry = "USA",
                BillingStreet = "321 Date St",
                BillingCity = "Phoenix",
                BillingPostalCode = "85001",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerEmail = "test@example.com",
                CustomerContactNumber = "+1234567890",
                OrderStatus = OrderStatus.Shipped,
                OrderDate = baseDate.AddDays(-1), // Within range
                Currency = Currency.USD,
                Subtotal = 150.00,
                TotalAmount = 165.00,
                ShippingStreet = "321 Date St",
                ShippingCity = "Phoenix",
                ShippingPostalCode = "85001",
                ShippingCountry = "USA",
                BillingStreet = "321 Date St",
                BillingCity = "Phoenix",
                BillingPostalCode = "85001",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            }
        };

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Orders.AddRangeAsync(orders);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<CustomerOrderDto>(It.IsAny<Order>()))
            .Returns((Order order) => new CustomerOrderDto
            {
                OrderId = order.Id,
                CustomerEmail = order.CustomerEmail,
                OrderStatus = order.OrderStatus.ToString(),
                OrderDate = order.OrderDate,
                Subtotal = order.Subtotal,
                Total = order.TotalAmount
            });

        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns((Customer customer) => new CustomerProfileDto
            {
                Id = customer.Id,
                Email = customer.Email,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Role = customer.Role
            });

        // Act
        var result = await _orderService.SearchOrdersAsync(
            placedAtStartRange: baseDate.AddDays(-5),
            placedAtEndRange: baseDate
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, dto =>
        {
            Assert.True(dto.OrderDate >= baseDate.AddDays(-5));
            Assert.True(dto.OrderDate <= baseDate);
        });
    }

    [Fact]
    public async Task SearchOrdersAsync_ShouldReturnData_WithPagination()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            Email = "test@example.com",
            Password = "hash",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Customer
        };

        var orders = new List<Order>();
        for (int i = 1; i <= 25; i++)
        {
            orders.Add(new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerEmail = "test@example.com",
                CustomerContactNumber = "+1234567890",
                OrderStatus = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow.AddDays(-i), // Different dates for ordering
                Currency = Currency.USD,
                Subtotal = 100.00 + i,
                TotalAmount = 110.00 + i,
                ShippingStreet = $"{100 + i} Page St",
                ShippingCity = "Denver",
                ShippingPostalCode = "80201",
                ShippingCountry = "USA",
                BillingStreet = $"{100 + i} Page St",
                BillingCity = "Denver",
                BillingPostalCode = "80201",
                BillingCountry = "USA",
                Products = new List<PurchasedProduct>()
            });
        }

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Orders.AddRangeAsync(orders);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<CustomerOrderDto>(It.IsAny<Order>()))
            .Returns((Order order) => new CustomerOrderDto
            {
                OrderId = order.Id,
                CustomerEmail = order.CustomerEmail,
                OrderStatus = order.OrderStatus.ToString(),
                OrderDate = order.OrderDate,
                Subtotal = order.Subtotal,
                Total = order.TotalAmount
            });

        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns((Customer customer) => new CustomerProfileDto
            {
                Id = customer.Id,
                Email = customer.Email,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Role = customer.Role
            });

        // Act
        var result = await _orderService.SearchOrdersAsync(page: 2, pageSize: 20);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count()); // Should only have 5 items on page 2 (items 21-25)
    }

    [Fact]
    public async Task SearchOrdersAsync_ShouldReturnEmptyList_WhenNoMatches()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            Email = "test@example.com",
            Password = "hash",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Customer
        };

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerEmail = "test@example.com",
            CustomerContactNumber = "+1234567890",
            OrderStatus = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            Currency = Currency.USD,
            Subtotal = 100.00,
            TotalAmount = 110.00,
            ShippingStreet = "999 Empty St",
            ShippingCity = "Austin",
            ShippingPostalCode = "73301",
            ShippingCountry = "USA",
            BillingStreet = "999 Empty St",
            BillingCity = "Austin",
            BillingPostalCode = "73301",
            BillingCountry = "USA",
            Products = new List<PurchasedProduct>()
        };

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<CustomerOrderDto>(It.IsAny<Order>()))
            .Returns((Order order) => new CustomerOrderDto
            {
                OrderId = order.Id,
                CustomerEmail = order.CustomerEmail,
                OrderStatus = order.OrderStatus.ToString(),
                OrderDate = order.OrderDate,
                Subtotal = order.Subtotal,
                Total = order.TotalAmount
            });

        _mapperMock.Setup(m => m.Map<CustomerProfileDto>(It.IsAny<Customer>()))
            .Returns((Customer customer) => new CustomerProfileDto
            {
                Id = customer.Id,
                Email = customer.Email,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Role = customer.Role
            });

        // Act
        var result = await _orderService.SearchOrdersAsync(customerEmailPattern: "nonexistent");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}