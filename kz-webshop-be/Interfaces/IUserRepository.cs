using kz_webshop_be.DTOs;
using kz_webshop_be.Enums;
using kz_webshop_be.Models;

namespace kz_webshop_be.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByFirebaseUidAsync(string firebaseUid);
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(UserDto user);
        Task SaveChangesAsync();
        Task<User?> GetUserWithDetailsAsync(Guid id);
        Task<Guid?> GetInternalUserIdByFirebaseAuthAsync(System.Security.Claims.ClaimsPrincipal user);
        Task AddLikedItemAsync(Guid userId, Guid productId, ProductType productType);
        Task<IEnumerable<LikedItem>> GetLikedItemsAsync(Guid userId);
        Task RemoveLikedItemAsync(Guid userId, Guid likedItemId);
        Task AddRecentlyViewedAsync(Guid? userId, Guid productId, ProductType productType);
        Task<List<RecentlyViewedItem>> GetRecentlyViewedAsync(Guid? userId, int maxCount = 5);
    }
}

