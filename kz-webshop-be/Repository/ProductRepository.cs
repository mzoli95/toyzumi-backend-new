using kz_webshop_be.DTOs;
using kz_webshop_be.Enums;
using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using Microsoft.EntityFrameworkCore;

namespace kz_webshop_be.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ShoppingCartProductDto?> GetProductByIdAsync(Guid productId, ProductType productType)
        {
            return productType switch
            {
                ProductType.FunkoPop => await _context.FunkoPops
                    .Where(p => p.Id == productId)
                    .Select(p => new ShoppingCartProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        MainImageUrl = p.MainImageUrl,
                        OriginalPrice = p.Price,
                        isPreorder = p.IsPreorder,
                        ReleaseDate = p.ReleaseDate

                    })
                    .FirstOrDefaultAsync(),
                ProductType.Labubu => await _context.Labubus
                    .Where(p => p.Id == productId)
                    .Select(p => new ShoppingCartProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        MainImageUrl = p.MainImageUrl,
                        OriginalPrice = p.Price,
                        ReleaseDate = p.ReleaseDate 

                    })
                    .FirstOrDefaultAsync(),
                _ => null
            };
        }
    }
}