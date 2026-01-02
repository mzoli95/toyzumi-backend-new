using AutoMapper;
using kz_webshop_be.DTOs;
using kz_webshop_be.Enums;
using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using Microsoft.EntityFrameworkCore;

namespace kz_webshop_be.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<User?> GetUserByFirebaseUidAsync(string firebaseUid)
            => await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

        public async Task<User?> GetUserByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);


        public async Task AddUserAsync(UserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            await _context.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public async Task<User?> GetUserWithDetailsAsync(Guid id)
            => await _context.Users
                .Include(u => u.Addresses)
                .Include(u => u.Orders)
                .Include(u => u.ShoppingCartItems)
                .Include(u => u.UserDiscountCodes)
                .Include(u => u.LikedItems)
                .FirstOrDefaultAsync(u => u.Id == id);

        public async Task<Guid?> GetInternalUserIdByFirebaseAuthAsync(System.Security.Claims.ClaimsPrincipal user)
        {
            // Próbáld ki, melyik claimben van a Firebase UID
            var firebaseUid = user.FindFirst("user_id")?.Value
                ?? user.FindFirst("firebase_uid")?.Value
                ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
                return null;

            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            return dbUser?.Id;
        }

        public async Task AddLikedItemAsync(Guid userId, Guid productId, ProductType productType)
        {
            var likedItem = new LikedItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = productId,
                ProductType = productType
            };
            await _context.LikedItems.AddAsync(likedItem);
        }
        public async Task<IEnumerable<LikedItem>> GetLikedItemsAsync(Guid userId)
           => await _context.LikedItems.Where(li => li.UserId == userId).ToListAsync();

        public async Task RemoveLikedItemAsync(Guid userId, Guid likedItemId)
        {
            var item = await _context.LikedItems.FirstOrDefaultAsync(li => li.UserId == userId && li.ProductId == likedItemId);
            if (item != null)
                _context.LikedItems.Remove(item);
        }

        public async Task AddRecentlyViewedAsync(Guid? userId, Guid productId, ProductType productType)
        {
            if (userId == null || userId == Guid.Empty)
                return;

            var existing = await _context.RecentlyViewedItems
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId && x.ProductType == productType);

            if (existing != null)
            {
                // Csak a dátumot frissítjük
                existing.ViewedAt = DateTime.UtcNow;
                _context.RecentlyViewedItems.Update(existing);
            }
            else
            {
                var all = await _context.RecentlyViewedItems
                    .Where(x => x.UserId == userId)
                    .OrderBy(x => x.ViewedAt)
                    .ToListAsync();

                if (all.Count >= 5)
                {
                    var oldest = all.First();
                    _context.RecentlyViewedItems.Remove(oldest);
                }

                var viewed = new RecentlyViewedItem
                {
                    UserId = userId.Value,
                    ProductId = productId,
                    ProductType = productType,
                    ViewedAt = DateTime.UtcNow
                };
                _context.RecentlyViewedItems.Add(viewed);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<RecentlyViewedItem>> GetRecentlyViewedAsync(Guid? userId, int maxCount = 5)
        {
            if (userId == null || userId == Guid.Empty)
                return new List<RecentlyViewedItem>();

            return await _context.RecentlyViewedItems
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.ViewedAt)
                .Take(maxCount)
                .ToListAsync();
        }
    }
}