using kz_webshop_be.DTOs;

namespace kz_webshop_be.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByFirebaseUidAsync(string firebaseUid);
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(UserDto user);
        Task SaveChangesAsync();
        Task<User?> GetUserWithDetailsAsync(Guid id);
    }
}

