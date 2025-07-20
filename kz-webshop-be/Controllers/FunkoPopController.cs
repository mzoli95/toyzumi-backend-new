using Microsoft.AspNetCore.Mvc;
using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using kz_webshop_be.DTOs;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;

namespace kz_webshop_be.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FunkoPopController : ControllerBase
{
    private readonly IFunkoPopRepository _funkoPopRepository;
    private readonly ApplicationDbContext _context;

    public FunkoPopController(
        IFunkoPopRepository funkoPopRepository,
        ApplicationDbContext context)
    {
        _funkoPopRepository = funkoPopRepository;
        _context = context;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FunkoPopDto>>> GetAll()
    {
        var funkos = await _funkoPopRepository.GetAllAsync();
        return Ok(funkos.Select(MapToDetailDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FunkoPopDto>> GetById(Guid id)
    {
        var funko = await _funkoPopRepository.GetByIdAsync(id);
        if (funko == null)
            return NotFound();
        return Ok(MapToDetailDto(funko));
    }

    [HttpPost]
    public async Task<ActionResult<FunkoPopDto>> Create([FromBody] FunkoPopDto dto)
    {
        var funkoPop = MapFromDto(dto);
        funkoPop.Id = Guid.NewGuid();
        funkoPop.CreatedAt = DateTime.UtcNow;
        funkoPop.UpdatedAt = DateTime.UtcNow;

        // Itt állítsd be a képek ProductId-ját
        if (funkoPop.Images != null)
        {
            foreach (var image in funkoPop.Images)
            {
                image.ProductId = funkoPop.Id;
                    image.Id = Guid.NewGuid();

                _context.ProductImages.Add(image);
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

        // Handle images - remove old ones and add new ones
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
                    Id = imageDto.Id != Guid.Empty ? imageDto.Id : Guid.NewGuid(),
                    ProductId = id,
                    Url = imageDto.Url,
                    SortOrder = imageDto.SortOrder
                };
                _context.ProductImages.Add(image);
            }
        }

        // Handle tags - remove old ones and add new ones
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
                    Id = tagDto.Id != Guid.Empty ? tagDto.Id : Guid.NewGuid(),
                    FunkoPopId = id,
                    Name = tagDto.Name
                };
                _context.FunkoPopTags.Add(tag);
            }
        }

        // Frissítés
        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.SalePrice = dto.SalePrice;
        existing.MainImageUrl = dto.MainImageUrl;
        existing.Category = dto.Category;
        existing.Franchise = dto.Franchise;
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

        // Kapcsolt entitások szinkronizálása (ha szükséges, implementáld a repository-ban!)

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
        IsUsed = f.IsUsed,
        Stock = f.Stock,
        ReleaseDate = f.ReleaseDate,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt,
        IsActive = f.IsActive,
        IsVisible = f.IsVisible,
        MaxOrderQuantity = f.MaxOrderQuantity,
        MinOrderQuantity = f.MinOrderQuantity,
        Images = f.Images?.Select(i => new ProductImageDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            Url = i.Url,
            SortOrder = i.SortOrder
        }).ToList(),
        FunkoPopTags = f.FunkoPopTags?.Select(t => new FunkoPopTagDto
        {
            Id = t.Id,
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
        IsUsed = dto.IsUsed,
        Stock = dto.Stock,
        ReleaseDate = dto.ReleaseDate,
        IsActive = dto.IsActive,
        IsVisible = dto.IsVisible,
        MaxOrderQuantity = dto.MaxOrderQuantity,
        MinOrderQuantity = dto.MinOrderQuantity,
        Images = dto.Images?.Select(i => new ProductImage
        {
            Id = i.Id != Guid.Empty ? i.Id : Guid.NewGuid(),
            ProductId = i.ProductId,
            Url = i.Url,
            SortOrder = i.SortOrder
        }).ToList(),
        FunkoPopTags = dto.FunkoPopTags?.Select(t => new FunkoPopTag
        {
            Id = t.Id != Guid.Empty ? t.Id : Guid.NewGuid(),
            Name = t.Name
        }).ToList(),
        RelatedProducts = dto.RelatedProducts?.Select(r => new RelatedProduct
        {
            ProductId = r.ProductId,
            ProductType = r.ProductType,
            RelatedToId = r.RelatedToId,
            RelatedToType = r.RelatedToType
        }).ToList(),
        Reviews = dto.Reviews?.Select(r => new ProductReview
        {
            Id = r.Id != Guid.Empty ? r.Id : Guid.NewGuid(),
            UserId = r.UserId,
            Stars = r.Stars,
            ReviewText = r.ReviewText,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList(),
        Comments = dto.Comments?.Select(c => new ProductComment
        {
            Id = c.Id != Guid.Empty ? c.Id : Guid.NewGuid(),
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

        // Csak képfájlokat engedélyezünk
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

        return Ok(results.ToArray()); // <-- Itt tömb lesz a válasz
    }
}