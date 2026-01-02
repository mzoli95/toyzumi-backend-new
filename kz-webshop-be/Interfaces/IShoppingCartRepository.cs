using kz_webshop_be.Enums;
using kz_webshop_be.Models;

namespace kz_webshop_be.Interfaces
{
    public interface IShoppingCartRepository
    {
        Task<ShoppingCartItem> SetItemQuantityAsync(Guid userId, Guid itemId, int newQuantity);
        Task<ShoppingCartItem> AddOrUpdateItemAsync(Guid userId, Guid productId, ProductType productType, int quantity, decimal unitPrice);
        Task<List<ShoppingCartItem>> GetItemsAsync(Guid userId);
        Task RemoveItemAsync(Guid userId, Guid itemId);

    }
}