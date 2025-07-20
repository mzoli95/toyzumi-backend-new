using kz_webshop_be.DTOs;
using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kz_webshop_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;

        public UserController(ApplicationDbContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _userRepository.GetUserByFirebaseUidAsync(request.FirebaseUid) != null)
                return Conflict("Firebase UID already exists.");

            if (await _userRepository.GetUserByEmailAsync(request.Email) != null)
                return Conflict("Email already exists.");

            var userDto = new UserDto
            {
                FirebaseUid = request.FirebaseUid,
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                ProfilePicture = request.ProfilePicture,
                Role = RoleState.User, 
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                Points = 0,
                Username = request.Username
            };

            await _userRepository.AddUserAsync(userDto);
            await _userRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = userDto.Id }, userDto);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _userRepository.GetUserByFirebaseUidAsync(loginRequest.FirebaseUid);

            if (user is null || user.IsDeleted)
            {
                return Unauthorized("User not registered or deleted.");
            }

            var fullUser = await _context.Users
                .Where(u => u.Id == user.Id)
                .Include(u => u.Addresses)
                .Include(u => u.Orders)
                .Include(u => u.ShoppingCartItems)
                .FirstOrDefaultAsync();

            return Ok(fullUser);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.Username,
                Role = u.Role,
                IsDeleted = u.IsDeleted,
                Points = u.Points,
                ProfilePicture = u.ProfilePicture,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();

            return Ok(userDtos);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch.");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Email = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Username = dto.Username;
            user.Phone = dto.Phone;
            user.ProfilePicture = dto.ProfilePicture;
            user.Role = dto.Role;
            user.IsDeleted = dto.IsDeleted;
            user.Points = dto.Points;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{userId:guid}/discounts")]
        public async Task<ActionResult<UserDiscountCodeDto>> AddDiscountToUser(Guid userId, [FromBody] DiscountCodeDto discountCodeDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            // Próbáljuk megkeresni a discount code-ot ID vagy kód alapján
            DiscountCode? discount = null;
            if (discountCodeDto.Id != Guid.Empty)
                discount = await _context.DiscountCodes.FindAsync(discountCodeDto.Id);

            if (discount == null && !string.IsNullOrWhiteSpace(discountCodeDto.Code))
                discount = await _context.DiscountCodes.FirstOrDefaultAsync(x => x.Code == discountCodeDto.Code);

            // Ha nem létezik, hozzuk létre
            if (discount == null)
            {
                discount = new DiscountCode
                {
                    Id = Guid.NewGuid(),
                    Code = discountCodeDto.Code ?? "",
                    DiscountAmount = discountCodeDto.DiscountAmount ?? 0,
                    IsPercentage = discountCodeDto.IsPercentage ?? true,
                    ExpirationDate = discountCodeDto.ExpirationDate ?? DateTime.Today
                };
                _context.DiscountCodes.Add(discount);
                await _context.SaveChangesAsync();
            }

            // Ellenőrizzük, hogy már hozzá van-e rendelve
            var alreadyExists = await _context.UserDiscountCodes
                .AnyAsync(x => x.UserId == userId && x.DiscountCodeId == discount.Id);
            if (alreadyExists)
                return Conflict("Discount code already assigned to user.");

            var userDiscount = new UserDiscountCode
            {
                UserId = userId,
                DiscountCodeId = discount.Id,
                UseCount = 0
            };

            _context.UserDiscountCodes.Add(userDiscount);
            await _context.SaveChangesAsync();

            var dto = new UserDiscountCodeDto
            {
                UserId = userDiscount.UserId,
                DiscountCodeId = userDiscount.DiscountCodeId,
                UseCount = userDiscount.UseCount,
                DiscountCode = new DiscountCodeDto
                {
                    Id = discount.Id,
                    Code = discount.Code,
                    DiscountAmount = discount.DiscountAmount,
                    IsPercentage = discount.IsPercentage,
                    ExpirationDate = discount.ExpirationDate
                }
            };

            return Ok(dto);
        }
        [HttpDelete("{userId:guid}/discounts/{discountCodeId:guid}")]
        public async Task<IActionResult> RemoveDiscountFromUser(Guid userId, Guid discountCodeId)
        {
            var userDiscount = await _context.UserDiscountCodes
                .FirstOrDefaultAsync(x => x.UserId == userId && x.DiscountCodeId == discountCodeId);

            if (userDiscount == null)
                return NotFound();

            _context.UserDiscountCodes.Remove(userDiscount);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{userId:guid}/discounts")]
        public async Task<ActionResult<IEnumerable<UserDiscountCodeDto>>> GetUserDiscountCodes(Guid userId)
        {
            var userDiscounts = await _context.UserDiscountCodes
                .Where(x => x.UserId == userId)
                .Include(x => x.DiscountCode)
                .ToListAsync();

            var result = userDiscounts.Select(x => new UserDiscountCodeDto
            {
                UserId = x.UserId,
                DiscountCodeId = x.DiscountCodeId,
                UseCount = x.UseCount,
                DiscountCode = x.DiscountCode == null ? null : new DiscountCodeDto
                {
                    Id = x.DiscountCode.Id,
                    Code = x.DiscountCode.Code,
                    DiscountAmount = x.DiscountCode.DiscountAmount,
                    IsPercentage = x.DiscountCode.IsPercentage,
                    ExpirationDate = x.DiscountCode.ExpirationDate
                }
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.UserDiscountCodes)
                    .ThenInclude(udc => udc.DiscountCode)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                FirebaseUid = user.FirebaseUid,
                Email = user.Email,
                Role = user.Role,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                ProfilePicture = user.ProfilePicture,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsDeleted = user.IsDeleted,
                Points = user.Points,
                DiscountCodes = user.UserDiscountCodes?.Select(x => new UserDiscountCodeDto
                {
                    UserId = x.UserId,
                    DiscountCodeId = x.DiscountCodeId,
                    UseCount = x.UseCount,
                    DiscountCode = x.DiscountCode == null ? null : new DiscountCodeDto
                    {
                        Id = x.DiscountCode.Id,
                        Code = x.DiscountCode.Code,
                        DiscountAmount = x.DiscountCode.DiscountAmount,
                        IsPercentage = x.DiscountCode.IsPercentage,
                        ExpirationDate = x.DiscountCode.ExpirationDate
                    }
                }).ToList()
            };

            return Ok(userDto);
        }
    }
}