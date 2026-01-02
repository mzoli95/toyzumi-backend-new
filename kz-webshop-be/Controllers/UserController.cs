using kz_webshop_be.DTOs;
using kz_webshop_be.Enums;
using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using kz_webshop_be.Repository;
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
        private readonly IEmailService _emailService;
        private readonly IUserIdentityService _identityService;

        public UserController(
            ApplicationDbContext context,
            IUserRepository userRepository,
            IEmailService emailService,
            IUserIdentityService identityService)
        {
            _context = context;
            _userRepository = userRepository;
            _emailService = emailService;
            _identityService = identityService;
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
        public async Task<ActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _context.Users.AsQueryable();
            var totalItems = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Username) // vagy bármilyen rendezés
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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

            return Ok(new
            {
                items = userDtos,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalItems
                }
            });
        }

        [HttpGet("search-users")]
        public async Task<ActionResult> SearchUsers([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query is required.");

            var loweredQuery = query.ToLower();

            var userQuery = _context.Users
                .Where(u =>
                    (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(loweredQuery)) ||
                    (!string.IsNullOrEmpty(u.Username) && u.Username.ToLower().Contains(loweredQuery)) ||
                    (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.ToLower().Contains(loweredQuery)) ||
                    (!string.IsNullOrEmpty(u.LastName) && u.LastName.ToLower().Contains(loweredQuery))
                );

            var totalItems = await userQuery.CountAsync();

            var users = await userQuery
                .OrderBy(u => u.Username) // vagy bármilyen rendezés
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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

            return Ok(new
            {
                items = userDtos,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalItems
                }
            });
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

        //TODO firebaseuid nélkül
        [HttpGet("get-user/{firebaseUid}")]
        public async Task<ActionResult<UserDto>> GetUserByFirebaseUid(string firebaseUid)
        {
            var user = await _context.Users
                .Include(u => u.UserDiscountCodes)
                    .ThenInclude(udc => udc.DiscountCode)
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
                return NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                FirebaseUid = user.FirebaseUid,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                ProfilePicture = user.ProfilePicture,
                Role = user.Role,
                IsDeleted = user.IsDeleted,
                Points = user.Points,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsNewsletterSubscribed = user.IsNewsletterSubscribed,
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
                UseCount = 1
            };

            _context.UserDiscountCodes.Add(userDiscount);
            await _context.SaveChangesAsync();

            var discountText = discount.IsPercentage
                ? $"{discount.DiscountAmount}%"
                : $"{discount.DiscountAmount} Ft";

            var htmlBody = $@"
            <html>
              <body style=' font-family: Arial, sans-serif; color: #16425b; margin:0; padding:0;'>
                <table width='100%' cellpadding='0' cellspacing='0' >
                  <tr>
                    <td align='center'>
                      <table width='500' cellpadding='0' cellspacing='0' style='background-color: #e8e8e8; border-radius: 8px; box-shadow: 0 2px 8px #81c3d7; margin: 32px 0;'>
                        <tr>
                          <td style='background-color: #2f6690; padding: 24px 0; border-radius: 8px 8px 0 0; text-align: center;'>
                            <h1 style='color: #fff; margin: 0;'>
                              <a href='https://toyzumi.hu' style='color: #fff; text-decoration: none;'>Toyzumi</a>
                            </h1>
                          </td>
                        </tr>
                        <tr>
                          <td style='padding: 32px;'>
                            <h2 style='color: #3a7ca5;'>Kedves {user.Username}!</h2>
                            <p style='font-size: 16px; color: #16425b;'>
                              Örömmel értesítünk, hogy új kedvezménykódot kaptál!
                            </p>
                            <div style='background-color: #81c3d7; color: #16425b; padding: 16px; border-radius: 6px; text-align: center; font-size: 20px; font-weight: bold; margin: 24px 0;'>
                              {discount.Code}
                            </div>
                            <p style='font-size: 15px; color: #16425b;'>
                              <strong>Kedvezmény:</strong> {discountText}<br>
                              <strong>Lejárat:</strong> {discount.ExpirationDate.ToString("yyyy.MM.dd")}
                            </p>
                            <p style='font-size: 15px; color: #16425b; margin-top: 16px;'>
                              <em>Ne hagyd ki! Vásárolj most, és használd fel a kedvezménykódot a következő rendelésednél.</em>
                            </p>
                            <p style='font-size: 14px; color: #16425b; margin-top: 24px;'>
                              A kódot a <strong>felhasználói fiókodban</strong> tudod beváltani, a <strong>Kuponok</strong> menüpontban.
                            </p>
                            <div style='text-align: center; margin-top: 32px;'>
                              <a href='https://toyzumi.hu/user/vouchers' style='display: inline-block; background-color: #2f6690; color: #fff; padding: 12px 32px; border-radius: 6px; text-decoration: none; font-size: 16px; font-weight: bold;'>
                                Kattints ide a beváltáshoz
                              </a>
                            </div>
                          </td>
                        </tr>
                        <tr>
                          <td style='background-color: #16425b; color: #d9dcd6; text-align: center; padding: 16px; border-radius: 0 0 8px 8px; font-size: 13px;'>
                            Toyzumi &copy; 2025
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
              </body>
            </html>
            ";

            await _emailService.SendEmailAsync(
                user.Email,
                "Toyzumi - Új kedvezménykódot kaptál!",
                htmlBody
            );

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

        [HttpPost("{userId:guid}/favorites")]
        public async Task<IActionResult> AddToFavorites(Guid userId, [FromBody] LikedItemDto likedItemDto)
        {
            var user = await _userRepository.GetUserWithDetailsAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var likedItems = await _userRepository.GetLikedItemsAsync(userId);
            if (likedItems.Any(x => x.ProductId == likedItemDto.ProductId && x.ProductType == likedItemDto.ProductType))
                return Conflict("Item already in favorites.");

            await _userRepository.AddLikedItemAsync(userId, likedItemDto.ProductId, likedItemDto.ProductType);
            await _userRepository.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{userId:guid}/favorites")]
        public async Task<ActionResult<IEnumerable<LikedItemDto>>> GetFavorites(Guid userId)
        {
            var likedItems = await _userRepository.GetLikedItemsAsync(userId);
            var result = likedItems.Select(x => new LikedItemDto
            {
                ProductId = x.ProductId,
                ProductType = x.ProductType
            }).ToList();

            return Ok(result);
        }

        [HttpDelete("{userId:guid}/favorites/{likedItemId:guid}")]
        public async Task<IActionResult> RemoveFromFavorites(Guid userId, Guid likedItemId)
        {
            await _userRepository.RemoveLikedItemAsync(userId, likedItemId);
            await _userRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("recently-viewed")]
        public async Task<IActionResult> AddRecentlyViewed([FromBody] RecentlyViewedItemDto dto)
        {
            var userId = await _userRepository.GetInternalUserIdByFirebaseAuthAsync(User);
            if (userId == null || userId == Guid.Empty)
                return Unauthorized("User not found.");

            await _userRepository.AddRecentlyViewedAsync(userId, dto.ProductId, dto.ProductType);
            return Ok();
        }


        [HttpPost("subscribe-newsletter-by-email")]
        public async Task<IActionResult> SubscribeNewsletterByEmail([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found.");

            user.IsNewsletterSubscribed = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Feliratkozás sikeres." });
        }


        [HttpPost("subscribe-newsletter")]
        public async Task<IActionResult> SubscribeNewsletter()
        {
            var userId = await _identityService.GetInternalUserIdAsync(User);
            if (userId == Guid.Empty)
                return Unauthorized("User not found.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.IsNewsletterSubscribed = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Feliratkozás sikeres." });
        }

        [HttpPost("unsubscribe-newsletter")]
        public async Task<IActionResult> UnsubscribeNewsletter()
        {
            var userId = await _identityService.GetInternalUserIdAsync(User);
            if (userId == Guid.Empty)
                return Unauthorized("User not found.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.IsNewsletterSubscribed = false;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Leiratkozás sikeres." });
        }    
    }
}