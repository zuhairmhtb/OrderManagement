using Microsoft.EntityFrameworkCore;
using OrderManagement.Web.Services;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Moq;
using Xunit;
using OrderManagement.Database.Context;
using OrderManagement.Database.Seeds;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;

namespace OrderManagement.Web.Tests.Services;
public class ProductServiceTest
{
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProductService _productService;
    private readonly ApplicationDbContext _dbContext;

    public ProductServiceTest()
    {
        _loggerMock = new Mock<ILogger<OrderService>>();
        _mapperMock = new Mock<IMapper>();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _productService = new ProductService(_loggerMock.Object, _mapperMock.Object, _dbContext);
        
    }

    [Fact]
    public async Task GetProducts_ReturnsAllProducts()
    {   ///Arrange
        _dbContext.Products.AddRange(ProductSeed.GetProducts(10));
        _dbContext.SaveChanges();

        // Act
        var result = await _productService.GetProducts();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_dbContext.Products.Count(), result.Count());
    }
}