using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Product;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Services;

public class ProductService : IProductService
{
	private readonly ILogger<OrderService> _logger;
	private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;

	public ProductService(ILogger<OrderService> logger, IMapper mapper, ApplicationDbContext dbContext)
	{
		_logger = logger;
		_mapper = mapper;
        _dbContext = dbContext;
	}

    public async Task<IEnumerable<ProductDetailDto>> GetProducts()
    {
		_logger.LogInformation("Fetching products from database");
        return await _dbContext.Products.Select(x => _mapper.Map<ProductDetailDto>(x)).ToListAsync();
    }



	
}
