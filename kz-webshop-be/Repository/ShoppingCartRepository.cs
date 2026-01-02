using kz_webshop_be.Enums;
using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using Microsoft.EntityFrameworkCore;

namespace kz_webshop_be.Repository
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ShoppingCartItem> AddOrUpdateItemAsync(Guid userId, Guid productId, ProductType productType, int quantity, decimal unitPrice)
        {
            var item = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId && x.ProductType == productType);

            if (item == null)
            {
                item = new ShoppingCartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    ProductType = productType,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ShoppingCartItems.Add(item);
            }
            else
            {
                item.Quantity += quantity; // NÖVELÉS!
                item.UnitPrice = unitPrice;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return item;
        }


        public async Task<ShoppingCartItem> SetItemQuantityAsync(Guid userId, Guid itemId, int newQuantity)
        {
            var item = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == itemId);

            if (item == null)
                throw new InvalidOperationException("Cart item not found.");

            item.Quantity = newQuantity; 
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<List<ShoppingCartItem>> GetItemsAsync(Guid userId)
        {
            return await _context.ShoppingCartItems
                .Where(i => i.UserId == userId)
                .ToListAsync();
        }

        public async Task RemoveItemAsync(Guid userId, Guid itemId)
        {
            var item = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(i => i.UserId == userId && i.Id == itemId);
            if (item != null)
            {
                _context.ShoppingCartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}