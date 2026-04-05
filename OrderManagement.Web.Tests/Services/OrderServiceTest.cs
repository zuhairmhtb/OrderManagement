using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Database.Commands.Order;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Order;
using OrderManagement.Web.Services;

namespace OrderManagement.Web.Tests.Services;
public class OrderServiceTest: IDisposable
{
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ApplicationDbContext _dbContext;
    private readonly OrderService _orderService;

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
    }

    [Fact]
    public async Task PlaceOrderAsync_ShouldPublishPlaceOrderCommand()
    {
        // Arrange
        var command = new PlaceOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Products = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = 1, Quantity = 2 }
            }
        };

        // Act
        var result = await _orderService.PlaceOrderAsync(command);



        // Assert
        Assert.Equal(OrderStatus.Pending, result);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<PlaceOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
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