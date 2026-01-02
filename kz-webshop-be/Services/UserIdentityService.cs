using kz_webshop_be.Interfaces;
using kz_webshop_be.Repository;

namespace kz_webshop_be.Services
{
    public class UserIdentityService : IUserIdentityService
    {
        private readonly IUserRepository _userRepo;

        public UserIdentityService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public string? GetFirebaseUidFromClaims(System.Security.Claims.ClaimsPrincipal user)
        {
            return user.FindFirst("user_id")?.Value
                ?? user.FindFirst("firebase_uid")?.Value
                ?? user.FindFirst("sub")?.Value;
        }

        public async Task<Guid> GetInternalUserIdAsync(System.Security.Claims.ClaimsPrincipal user)
        {
            var firebaseUid = GetFirebaseUidFromClaims(user);
            if (string.IsNullOrEmpty(firebaseUid))
                return Guid.Empty;

            var dbUser = await _userRepo.GetUserByFirebaseUidAsync(firebaseUid);
            return dbUser?.Id ?? Guid.Empty;
        }
    }
}
