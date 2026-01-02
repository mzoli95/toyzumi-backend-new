using Microsoft.AspNetCore.Mvc;
using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using kz_webshop_be.DTOs;
using kz_webshop_be.Enums;
using kz_webshop_be.Repository;
using Microsoft.EntityFrameworkCore;

namespace kz_webshop_be.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LabubuController : ControllerBase
{
    private readonly ILabubuRepository _labubuRepository;
    private readonly IUserRepository _userRepository;

    public LabubuController(
        ILabubuRepository labubuRepository,
        IUserRepository userRepository)
    {
        _labubuRepository  = labubuRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LabubuDto>>> GetAll()
    {
        var labubus = await _labubuRepository.GetAllAsync();
        return Ok(labubus.Select(MapToDetailDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LabubuDto>> GetById(Guid id)
    {
        var labubu = await _labubuRepository.GetByIdAsync(id);
        if (labubu == null)
            return NotFound();

        var userId = await _userRepository.GetInternalUserIdByFirebaseAuthAsync(User);
        if (userId != null && userId != Guid.Empty)
            await _userRepository.AddRecentlyViewedAsync(userId.Value, id, ProductType.Labubu);

        return Ok(MapToDetailDto(labubu));
    }

    [HttpPost]
    public async Task<ActionResult<LabubuDto>> Create([FromBody] LabubuDto dto)
    {
        var labubu = MapFromDto(dto);
        labubu.Id = Guid.NewGuid();
        labubu.CreatedAt = DateTime.UtcNow;
        labubu.UpdatedAt = DateTime.UtcNow;

        await _labubuRepository.AddAsync(labubu);
        await _labubuRepository.SaveChangesAsync();

        var created = await _labubuRepository.GetByIdAsync(labubu.Id);
        return CreatedAtAction(nameof(GetById), new { id = labubu.Id }, MapToDetailDto(created!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] LabubuDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var existing = await _labubuRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        // Frissítés
        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.SalePrice = dto.SalePrice;
        existing.MainImageUrl = dto.MainImageUrl;
        existing.Series = dto.Series;
        existing.Edition = dto.Edition;
        existing.IsLimitedEdition = dto.IsLimitedEdition;
        existing.IsPreorder = dto.IsPreorder;
        existing.IsUsed = dto.IsUsed;
        existing.Stock = dto.Stock;
        existing.ReleaseDate = dto.ReleaseDate;
        existing.IsActive = dto.IsActive;
        existing.IsVisible = dto.IsVisible;
        existing.MaxOrderQuantity = dto.MaxOrderQuantity;
        existing.MinOrderQuantity = dto.MinOrderQuantity;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.IsOnSale = dto.IsOnSale;
        // Kapcsolt entitások szinkronizálása (ha szükséges, implementáld a repository-ban!)

        await _labubuRepository.UpdateAsync(existing);
        await _labubuRepository.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var labubu = await _labubuRepository.GetByIdAsync(id);
        if (labubu == null)
            return NotFound();

        await _labubuRepository.DeleteAsync(labubu);
        await _labubuRepository.SaveChangesAsync();
        return NoContent();
    }

    // --- Mapping segédfüggvények ---
    private static LabubuDto MapToDetailDto(Labubu l) => new LabubuDto
    {
        Id = l.Id,
        Name = l.Name,
        Description = l.Description,
        Price = l.Price,
        SalePrice = l.SalePrice,
        MainImageUrl = l.MainImageUrl,
        Series = l.Series,
        Edition = l.Edition,
        IsLimitedEdition = l.IsLimitedEdition,
        IsPreorder = l.IsPreorder,
        IsUsed = l.IsUsed,
        Stock = l.Stock,
        ReleaseDate = l.ReleaseDate,
        CreatedAt = l.CreatedAt,
        UpdatedAt = l.UpdatedAt,
        IsActive = l.IsActive,
        IsVisible = l.IsVisible,
        ProductType = ProductType.Labubu,
        MaxOrderQuantity = l.MaxOrderQuantity,
        MinOrderQuantity = l.MinOrderQuantity,
        Images = l.Images?.Select(i => new ProductImageDto
        {
            Id = i.Id,
            Url = i.Url,
            SortOrder = i.SortOrder
        }).ToList(),
        RelatedProducts = l.RelatedProducts?.Select(r => new RelatedProductDto
        {
            ProductId = r.ProductId,
            ProductType = r.ProductType,
            RelatedToId = r.RelatedToId,
            RelatedToType = r.RelatedToType
        }).ToList(),
        Reviews = l.Reviews?.Select(r => new ProductReviewDto
        {
            Id = r.Id,
            UserId = r.UserId,
            Stars = r.Stars,
            ReviewText = r.ReviewText,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList(),
        Comments = l.Comments?.Select(c => new ProductCommentDto
        {
            Id = c.Id,
            UserId = c.UserId,
            CommentText = c.CommentText,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList()
    };

    private static Labubu MapFromDto(LabubuDto dto) => new Labubu
    {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        SalePrice = dto.SalePrice,
        MainImageUrl = dto.MainImageUrl,
        Series = dto.Series,
        Edition = dto.Edition,
        IsLimitedEdition = dto.IsLimitedEdition,
        IsPreorder = dto.IsPreorder,
        IsUsed = dto.IsUsed,
        Stock = dto.Stock,
        ReleaseDate = dto.ReleaseDate,
        IsActive = dto.IsActive,
        IsVisible = dto.IsVisible,
        MaxOrderQuantity = dto.MaxOrderQuantity,
        MinOrderQuantity = dto.MinOrderQuantity,
        Images = dto.Images?.Select(i => new ProductImage
        {
            Id = i.Id ?? Guid.NewGuid(),
            Url = i.Url,
            SortOrder = i.SortOrder
        }).ToList(),
        RelatedProducts = dto.RelatedProducts?.Select(r => new RelatedProduct
        {
            ProductId = r.ProductId ?? new Guid(),
            ProductType = r.ProductType,
            RelatedToId = r.RelatedToId,
            RelatedToType = r.RelatedToType
        }).ToList(),
        Reviews = dto.Reviews?.Select(r => new ProductReview
        {
            Id = r.Id ?? Guid.NewGuid(),
            UserId = r.UserId,
            Stars = r.Stars,
            ReviewText = r.ReviewText,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList(),
        Comments = dto.Comments?.Select(c => new ProductComment
        {
            Id = c.Id ?? Guid.NewGuid(),
            UserId = c.UserId,
            CommentText = c.CommentText,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList()
    };
}