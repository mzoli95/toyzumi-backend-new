using System.Security.Claims;

namespace kz_webshop_be.Repository
{
    public interface IUserIdentityService
    {
        string? GetFirebaseUidFromClaims(ClaimsPrincipal user);
        Task<Guid> GetInternalUserIdAsync(ClaimsPrincipal user);
    }
}
