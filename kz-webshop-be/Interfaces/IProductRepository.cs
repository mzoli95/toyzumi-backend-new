using kz_webshop_be.DTOs;
using kz_webshop_be.Enums;
using kz_webshop_be.Models;

namespace kz_webshop_be.Interfaces
{
    public interface IProductRepository
    {
        Task<ShoppingCartProductDto?> GetProductByIdAsync(Guid productId, ProductType productType);
    }
}
