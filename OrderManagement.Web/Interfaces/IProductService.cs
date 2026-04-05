using OrderManagement.Database.Dtos.Product;

namespace OrderManagement.Web.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDetailDto>> GetProducts();
}