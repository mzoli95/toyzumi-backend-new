using AutoMapper;
using kz_webshop_be.DTOs;
using kz_webshop_be.Interfaces;
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

    }
}