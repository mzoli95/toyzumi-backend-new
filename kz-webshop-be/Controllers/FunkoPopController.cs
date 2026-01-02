using Microsoft.AspNetCore.Mvc;
using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using kz_webshop_be.DTOs;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using kz_webshop_be.Enums;

namespace kz_webshop_be.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FunkoPopController : ControllerBase
{
    private readonly IFunkoPopRepository _funkoPopRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUserRepository _userRepository;

    public FunkoPopController(
        IFunkoPopRepository funkoPopRepository,
        ApplicationDbContext context,
        IUserRepository userRepository)
    {
        _funkoPopRepository = funkoPopRepository;
        _context = context;
        _userRepository = userRepository;
    }

    [HttpGet("get-all")]
    public async Task<ActionResult<PagedResult<FunkoPopDto>>> GetAllFunkoPops(
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 20,
     [FromQuery] string? searchTerm = null,
     CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _context.FunkoPops
            .AsNoTracking()
            .Include(f => f.Images)
            .Include(f => f.Reviews)
            .Include(f => f.Comments)
            .Include(f => f.FunkoPopTags)
            .Include(f => f.Badges)
            .Include(f => f.RelatedProducts)
            .Where(d => !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowered = searchTerm.ToLower();
            query = query.Where(f =>
                (!string.IsNullOrEmpty(f.Name) && f.Name.ToLower().Contains(lowered)) ||
                (!string.IsNullOrEmpty(f.Description) && f.Description.ToLower().Contains(lowered))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var funkos = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<FunkoPopDto>
        {
            Items = funkos.Select(MapToDetailDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FunkoPopDto>> GetById(Guid id)
    {
        var funko = await _funkoPopRepository.GetByIdAsync(id);
        if (funko == null)
            return NotFound();

        var userId = await _userRepository.GetInternalUserIdByFirebaseAuthAsync(User);
        if (userId != null && userId != Guid.Empty)
            await _userRepository.AddRecentlyViewedAsync(userId.Value, id, ProductType.FunkoPop);

        return Ok(MapToDetailDto(funko));
    }

    [HttpPost]
    public async Task<ActionResult<FunkoPopDto>> Create([FromBody] FunkoPopDto dto)
    {
        var funkoPop = MapFromDto(dto);
        funkoPop.Id = Guid.NewGuid();
        funkoPop.CreatedAt = DateTime.UtcNow;
        funkoPop.UpdatedAt = DateTime.UtcNow;

        if (funkoPop.Images != null)
        {
            foreach (var image in funkoPop.Images)
            {
                image.ProductId = funkoPop.Id;
                image.Id = Guid.NewGuid();
                _context.ProductImages.Add(image);
            }
        }

        if (funkoPop.Badges != null)
        {
            foreach (var badge in funkoPop.Badges)
            {
                badge.FunkoPopId = funkoPop.Id;
                badge.Id = Guid.NewGuid();
                _context.FunkoPopBadges.Add(badge);
            }
        }

        await _funkoPopRepository.AddAsync(funkoPop);
        await _funkoPopRepository.SaveChangesAsync();

        var created = await _funkoPopRepository.GetByIdAsync(funkoPop.Id);
        return CreatedAtAction(nameof(GetById), new { id = funkoPop.Id }, MapToDetailDto(created!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] FunkoPopDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var existing = await _funkoPopRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        // Images
        if (dto.Images != null)
        {
            var existingImages = await _context.ProductImages
                .Where(i => i.ProductId == id)
                .ToListAsync();

            if (existingImages.Any())
                _context.ProductImages.RemoveRange(existingImages);

            foreach (var imageDto in dto.Images)
            {
                var image = new ProductImage
                {
                    Id = imageDto.Id ?? Guid.NewGuid(),
                    ProductId = id,
                    Url = imageDto.Url,
                    SortOrder = imageDto.SortOrder
                };
                _context.ProductImages.Add(image);
            }
        }

        // Badges
        if (dto.Badges != null)
        {
            var existingBadges = await _context.FunkoPopBadges
                .Where(b => b.FunkoPopId == id)
                .ToListAsync();

            if (existingBadges.Any())
                _context.FunkoPopBadges.RemoveRange(existingBadges);

            foreach (var badgeDto in dto.Badges)
            {
                var badgeEntity = new FunkoPopBadge
                {
                    Id = badgeDto.Id ?? Guid.NewGuid(),
                    FunkoPopId = id,
                    Badge = badgeDto.Badge
                };
                _context.FunkoPopBadges.Add(badgeEntity);
            }
        }

        // Tags
        if (dto.FunkoPopTags != null)
        {
            var existingTags = await _context.FunkoPopTags
                .Where(t => t.FunkoPopId == id)
                .ToListAsync();

            if (existingTags.Any())
                _context.FunkoPopTags.RemoveRange(existingTags);

            foreach (var tagDto in dto.FunkoPopTags)
            {
                var tag = new FunkoPopTag
                {
                    Id = tagDto.Id ?? Guid.NewGuid(),
                    FunkoPopId = id,
                    Name = tagDto.Name
                };
                _context.FunkoPopTags.Add(tag);
            }
        }

        // Update main properties
        existing.AverageRating = dto.AverageRating;
        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.SalePrice = dto.SalePrice;
        existing.MainImageUrl = dto.MainImageUrl;
        existing.Category = dto.Category;
        existing.Franchise = dto.Franchise;
        existing.IsLimitedEdition = dto.IsLimitedEdition;


        if (existing.ReleaseDate.HasValue && existing.ReleaseDate.Value > DateTime.UtcNow)
        {
            existing.IsPreorder = true;
        }
        else
        {
            existing.IsPreorder = false;
        }

        existing.CreatedBy = dto.CreatedBy;
        existing.UpdatedBy = dto.UpdatedBy;
        existing.IsExclusive = dto.IsExclusive;
        existing.IsChase = dto.IsChase;
        existing.Stock = dto.Stock;
        existing.ReleaseDate = dto.ReleaseDate;
        existing.IsActive = dto.IsActive;
        existing.IsNew = dto.IsNew;
        existing.IsVisible = dto.IsVisible;
        existing.MaxOrderQuantity = dto.MaxOrderQuantity;
        existing.MinOrderQuantity = dto.MinOrderQuantity;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.IsReStock = dto.IsReStock;
        existing.Dimensions = dto.Dimensions;
        await _funkoPopRepository.UpdateAsync(existing);
        await _funkoPopRepository.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var funko = await _funkoPopRepository.GetByIdAsync(id);
        if (funko == null)
            return NotFound();

        await _funkoPopRepository.DeleteAsync(funko);
        await _funkoPopRepository.SaveChangesAsync();
        return NoContent();
    }

    // --- Mapping segédfüggvények ---
    private static FunkoPopDto MapToDetailDto(FunkoPop f) => new FunkoPopDto
    {
        Id = f.Id,
        Name = f.Name,
        Description = f.Description,
        Price = f.Price,
        SalePrice = f.SalePrice,
        MainImageUrl = f.MainImageUrl,
        Category = f.Category,
        Franchise = f.Franchise,
        IsLimitedEdition = f.IsLimitedEdition,
        IsPreorder = f.IsPreorder,
        IsExclusive = f.IsExclusive,
        IsChase = f.IsChase,
        Stock = f.Stock,
        ReleaseDate = f.ReleaseDate,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt,
        IsActive = f.IsActive,
        IsVisible = f.IsVisible,
        AverageRating = f.AverageRating,
        Barcode = f.Barcode ,
        Brand = f.Brand,
        CreatedBy = f.CreatedBy ,
        Dimensions = f.Dimensions,
        IsDeleted = f.IsDeleted,
        IsNew = f.IsNew,
        IsOnSale = f.IsOnSale,
        IsAvailable = f.IsAvailable,
        IsReStock = f.IsReStock,
        Sku = f.Sku,
        UpdatedBy = f.UpdatedBy,
        ProductType = ProductType.FunkoPop,
        Weight = f.Weight,
        MaxOrderQuantity = f.MaxOrderQuantity,
        MinOrderQuantity = f.MinOrderQuantity,
        Images = f.Images?.Select(i => new ProductImageDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            Url = i.Url,
            SortOrder = i.SortOrder
        }).ToList(),
        Badges = f.Badges?.Select(b => new FunkoPopBadgeDto
        {
            Id = b.Id,
            FunkoPopId = b.FunkoPopId,
            Badge = b.Badge
        }).ToList(),
        FunkoPopTags = f.FunkoPopTags?.Select(t => new FunkoPopTagDto
        {
            Id = t.Id,
            FunkoPopId = t.FunkoPopId,
            Name = t.Name
        }).ToList(),
        RelatedProducts = f.RelatedProducts?.Select(r => new RelatedProductDto
        {
            ProductId = r.ProductId,
            ProductType = r.ProductType,
            RelatedToId = r.RelatedToId,
            RelatedToType = r.RelatedToType
        }).ToList(),
        Reviews = f.Reviews?.Select(r => new ProductReviewDto
        {
            Id = r.Id,
            UserId = r.UserId,
            Stars = r.Stars,
            ReviewText = r.ReviewText,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList(),
        Comments = f.Comments?.Select(c => new ProductCommentDto
        {
            Id = c.Id,
            UserId = c.UserId,
            CommentText = c.CommentText,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList()
    };

    private static FunkoPop MapFromDto(FunkoPopDto dto) => new FunkoPop
    {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        SalePrice = dto.SalePrice,
        MainImageUrl = dto.MainImageUrl,
        Category = dto.Category,
        Franchise = dto.Franchise,
        IsLimitedEdition = dto.IsLimitedEdition,
        IsPreorder = dto.IsPreorder,
        IsExclusive = dto.IsExclusive,
        IsChase = dto.IsChase,
        Stock = dto.Stock,
        IsAvailable = dto.IsAvailable,
        IsReStock= dto.IsReStock, 
        ReleaseDate = dto.ReleaseDate,
        IsActive = dto.IsActive,
        IsVisible = dto.IsVisible,
        IsDeleted = dto.IsDeleted,
        AverageRating = dto.AverageRating,
        Weight = dto.Weight,
        Barcode = dto.Barcode,
        Brand = dto.Brand,
        CreatedAt = dto.CreatedAt,
        CreatedBy = dto.CreatedBy,
        Dimensions = dto.Dimensions,
        IsNew = dto.IsNew,
        IsOnSale = dto.IsOnSale,
        Sku = dto.Sku,
        UpdatedAt = dto.UpdatedAt,
        UpdatedBy = dto.UpdatedBy,
        MaxOrderQuantity = dto.MaxOrderQuantity,
        MinOrderQuantity = dto.MinOrderQuantity,
        Images = dto.Images?.Select(i => new ProductImage
        {
            Id = i.Id ?? Guid.NewGuid(),
            ProductId = i.ProductId,
            Url = i.Url,
            SortOrder = i.SortOrder
        }).ToList(),
        Badges = dto.Badges?.Select(b => new FunkoPopBadge
        {
            Id = b.Id ?? Guid.NewGuid(),
            FunkoPopId = b.FunkoPopId,
            Badge = b.Badge
        }).ToList() ?? new List<FunkoPopBadge>(),
        FunkoPopTags = dto.FunkoPopTags?.Select(t => new FunkoPopTag
        {
            Id = t.Id ?? Guid.NewGuid(),
            FunkoPopId = t.FunkoPopId,
            Name = t.Name
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

    [HttpPost("upload-main-image")]
    public async Task<IActionResult> UploadMainImage([FromForm] IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("No image uploaded");

        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedContentTypes.Contains(image.ContentType))
            return BadRequest("Only image files are allowed.");

        var blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
        var containerClient = blobServiceClient.GetBlobContainerClient("funko-images");
        await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var blobClient = containerClient.GetBlobClient($"main-images/{fileName}");

        await using var stream = image.OpenReadStream();
        await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
        {
            ContentType = image.ContentType
        });

        var url = blobClient.Uri.ToString();

        return Ok(new { fileName, url });
    }

    [HttpPost("upload-images")]
    public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> images)
    {
        if (images == null || images.Count == 0)
            return BadRequest("No images uploaded");

        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        var blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
        var containerClient = blobServiceClient.GetBlobContainerClient("funko-images");
        await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

        var results = new List<ProductImageDto>();

        foreach (var image in images)
        {
            if (image == null || image.Length == 0)
                continue;

            if (!allowedContentTypes.Contains(image.ContentType))
                continue;

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var blobClient = containerClient.GetBlobClient($"main-images/{fileName}");

            await using var stream = image.OpenReadStream();
            await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = image.ContentType
            });

            var url = blobClient.Uri.ToString();
            results.Add(new ProductImageDto
            {
                Url = url,
                SortOrder = 0,
            });
        }

        if (results.Count == 0)
            return BadRequest("No valid images uploaded.");

        return Ok(results.ToArray());
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<string>>> Search([FromQuery] string term, [FromQuery] int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest("Search term is required.");

        var results = await _funkoPopRepository.SearchNamesAsync(term, maxResults);
        return Ok(results);
    }
}