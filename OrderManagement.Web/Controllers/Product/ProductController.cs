using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Controllers.Product;


[ApiController]
[Route("api/[controller]")]
public class ProductController: ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;
    private readonly IMapper _mapper;

    public ProductController(IProductService productService, ILogger<ProductController> logger, IMapper mapper)
    {
        _productService = productService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        _logger.LogInformation("Received request to get products");
        var products = await _productService.GetProducts();
        if(products == null || !products.Any())
        {
            _logger.LogInformation("No products found");
            return NotFound(new { Message = "No products found." });
        }
        return Ok(products);
    }
}