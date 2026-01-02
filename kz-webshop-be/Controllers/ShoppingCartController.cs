using kz_webshop_be.DTOs;
using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kz_webshop_be.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartRepository _cartRepo;
        private readonly IUserRepository _userRepo;
        private readonly IProductRepository _productRepo;

        public ShoppingCartController(
            IShoppingCartRepository cartRepo,
            IUserRepository userRepo,
            IProductRepository productRepo)
        {
            _cartRepo = cartRepo;
            _userRepo = userRepo;
            _productRepo = productRepo;
        }

        private string? GetFirebaseUidFromJwt()
        {
            return User.FindFirst("user_id")?.Value
                ?? User.FindFirst("firebase_uid")?.Value
                ?? User.FindFirst("sub")?.Value;
        }

        private async Task<Guid> GetInternalUserIdAsync()
        {
            var firebaseUid = GetFirebaseUidFromJwt();
            if (string.IsNullOrEmpty(firebaseUid))
                return Guid.Empty;

            var user = await _userRepo.GetUserByFirebaseUidAsync(firebaseUid);
            return user?.Id ?? Guid.Empty;
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = await GetInternalUserIdAsync();
            if (userId == Guid.Empty)
                return Unauthorized("User not found.");

            var cartItem = await _cartRepo.AddOrUpdateItemAsync(userId, dto.ProductId, dto.ProductType, dto.Quantity, dto.UnitPrice);
            var dtoItem = await ToDtoAsync(cartItem);
            return Ok(dtoItem);
        }


        [HttpDelete("{itemId}")]
        public async Task<IActionResult> RemoveFromCart(Guid itemId)
        {
            var userId = await GetInternalUserIdAsync();
            if (userId == Guid.Empty)
                return Unauthorized("User not found.");
            await _cartRepo.RemoveItemAsync(userId, itemId);
            return Ok();
        }


        [HttpGet]
        public async Task<ActionResult<ShoppingCartItemDto[]>> GetCart()
        {
            var userId = await GetInternalUserIdAsync();
            if (userId == Guid.Empty)
                return Unauthorized("User not found.");
            var items = await _cartRepo.GetItemsAsync(userId);
            var dtoItems = new List<ShoppingCartItemDto>();
            foreach (var item in items)
            {
                dtoItems.Add(await ToDtoAsync(item));
            }
            return Ok(dtoItems);
        }

        private async Task<ShoppingCartItemDto> ToDtoAsync(ShoppingCartItem item)
        {
            var product = await _productRepo.GetProductByIdAsync(item.ProductId, item.ProductType);
            return new ShoppingCartItemDto
            {
                Id = item.Id,
                UserId = item.UserId,
                ProductId = item.ProductId,
                ProductType = item.ProductType,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                AddedAt = item.AddedAt,
                UpdatedAt = item.UpdatedAt,
                Name = product?.Name ?? string.Empty,
                MainImageUrl = product?.MainImageUrl ?? string.Empty,
                OriginalPrice = product?.OriginalPrice,
                IsPreorder = product?.isPreorder,
                ReleaseDate = product?.ReleaseDate 

            };
        }

        [HttpPut("update/{itemId}")]
        public async Task<IActionResult> UpdateCartItem(Guid itemId, [FromBody] int newQuantity)
        {
            var userId = await GetInternalUserIdAsync();
            if (userId == Guid.Empty)
                return Unauthorized("User not found.");

            if (newQuantity < 1)
                return BadRequest("Quantity must be at least 1.");

            var updatedItem = await _cartRepo.SetItemQuantityAsync(userId, itemId, newQuantity);
            var dtoItem = await ToDtoAsync(updatedItem);
            return Ok(dtoItem);
        }
    }
}