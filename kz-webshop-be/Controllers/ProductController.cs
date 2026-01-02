using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using kz_webshop_be.DTOs;
using kz_webshop_be.Models;
using kz_webshop_be.Enums;
using kz_webshop_be.Interfaces;
using System.Linq;

namespace kz_webshop_be.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEnumService _enumService;

    public ProductController(ApplicationDbContext context, IEnumService enumService)
    {
        _context = context;
        _enumService = enumService;
    }

    private static IEnumerable<T> Paginate<T>(IEnumerable<T> source, int page, int pageSize)
    {
        return source.Skip((page - 1) * pageSize).Take(pageSize);
    }

    [HttpGet("top-favorites")]
    public async Task<ActionResult<IEnumerable<PromotionItemDto>>> GetTopFavorites()
    {
        var userId = await GetBackendUserIdAsync();
        var (favoriteFunkoIds, favoriteLabubuIds) = await GetUserFavoriteIdsAsync(userId);

        var topLiked = await _context.LikedItems
            .GroupBy(l => new { l.ProductId, l.ProductType })
            .Select(g => new
            {
                ProductId = g.Key.ProductId,
                ProductType = g.Key.ProductType,
                LikeCount = g.Count()
            })
            .OrderByDescending(x => x.LikeCount)
            .ThenBy(x => x.ProductType)
            .ToListAsync();

        var funkoIds = topLiked.Where(x => x.ProductType == ProductType.FunkoPop).Select(x => x.ProductId).ToList();
        var labubuIds = topLiked.Where(x => x.ProductType == ProductType.Labubu).Select(x => x.ProductId).ToList();

        var funkos = await _context.FunkoPops
            .AsNoTracking()
            .Where(f => funkoIds.Contains(f.Id))
            .ToListAsync();

        var labubus = await _context.Labubus
            .AsNoTracking()
            .Where(l => labubuIds.Contains(l.Id))
            .ToListAsync();

        var funkoDtos = funkos.Select(f =>
        {
            var dto = MapFunkoPopDto(f);
            dto.IsFavorite = favoriteFunkoIds.Contains(f.Id);
            return dto;
        }).ToList();

        var labubuDtos = labubus.Select(f =>
        {
            var dto = MapLabubuDto(f);
            dto.IsFavorite = favoriteFunkoIds.Contains(f.Id); 
            return dto;
        }).ToList();

        var items = new List<PromotionItemDto>();
        foreach (var item in topLiked)
        {
            if (item.ProductType == ProductType.FunkoPop)
            {
                var dto = funkoDtos.FirstOrDefault(f => f.Id == item.ProductId);
                if (dto != null)
                    items.Add(new PromotionItemDto
                    {
                        Type = ProductType.FunkoPop,
                        FunkoPop = dto,
                        Labubu = null,
                        IsFavorite = favoriteFunkoIds.Contains(item.ProductId)
                    });
            }
            else if (item.ProductType == ProductType.Labubu)
            {
                var dto = labubuDtos.FirstOrDefault(l => l.Id == item.ProductId);
                if (dto != null)
                    items.Add(new PromotionItemDto
                    {
                        Type = ProductType.Labubu,
                        FunkoPop = null,
                        Labubu = dto,
                        IsFavorite = favoriteLabubuIds.Contains(item.ProductId)
                    });
            }
        }

        return Ok(items);
    }

    [HttpGet("new")]
    public async Task<ActionResult<IEnumerable<PromotionItemDto>>> GetAllNew([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = await GetBackendUserIdAsync();
        var (favoriteFunkoIds, favoriteLabubuIds) = await GetUserFavoriteIdsAsync(userId);

        var funkos = await _context.FunkoPops
            .AsNoTracking()
            .Where(f => f.IsNew)
            .ToListAsync();

        var labubus = await _context.Labubus
            .AsNoTracking()
            .Where(l => l.IsNew)
            .ToListAsync();

        var funkoDtos = funkos.Select(f =>
        {
            var dto = MapFunkoPopDto(f);
            dto.IsFavorite = favoriteFunkoIds.Contains(f.Id);
            return dto;
        });

        var labubuDtos = labubus.Select(l =>
        {
            var dto = MapLabubuDto(l);
            dto.IsFavorite = favoriteLabubuIds.Contains(l.Id);
            return dto;
        });

        var all = funkoDtos
            .Select(dto => new PromotionItemDto
            {
                Type = ProductType.FunkoPop,
                FunkoPop = dto,
                Labubu = null
            })
            .Concat(labubuDtos.Select(dto => new PromotionItemDto
            {
                Type = ProductType.Labubu,
                FunkoPop = null,
                Labubu = dto
            }))
            .OrderByDescending(x => x.FunkoPop?.CreatedAt ?? x.Labubu?.CreatedAt);

        var paged = Paginate(all, page, pageSize).ToList();

        return Ok(paged);
    }
    [HttpGet("search")]
    public async Task<ActionResult> SearchProducts(
        [FromQuery] ProductFilterDto filter,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var userId = await GetBackendUserIdAsync();
        var (favoriteFunkoIds, favoriteLabubuIds) = await GetUserFavoriteIdsAsync(userId);

        // FunkoPop szűrés
        var funkoQuery = _context.FunkoPops.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            funkoQuery = funkoQuery.Where(p => p.Name.Contains(filter.SearchTerm) ||
                                               (p.Description != null && p.Description.Contains(filter.SearchTerm)));

        if (filter.Categories != null && filter.Categories.Any())
            funkoQuery = funkoQuery.Where(p => filter.Categories.Contains(Convert.ToInt32(p.Category)));

        if (filter.Franchises != null && filter.Franchises.Any())
            funkoQuery = funkoQuery.Where(p => filter.Franchises.Contains(Convert.ToInt32(p.Franchise ?? Franchise.Other)));

        if (filter.PriceRange != null)
        {
            var min = GetDecimalValue(filter.PriceRange, "Min");
            var max = GetDecimalValue(filter.PriceRange, "Max");
            if (min.HasValue)
                funkoQuery = funkoQuery.Where(p => p.Price >= min.Value);
            if (max.HasValue)
                funkoQuery = funkoQuery.Where(p => p.Price <= max.Value);
        }

        if (filter.Availability != null && filter.Availability.Any())
        {
            var availabilityConditions = filter.Availability.Select(av => av.ToLower()).ToList();
            funkoQuery = funkoQuery.Where(p =>
                (availabilityConditions.Contains("instock") && p.Stock > 0) ||
                (availabilityConditions.Contains("preorder") && p.IsPreorder == true) ||
                (availabilityConditions.Contains("outofstock") && p.Stock == 0 && p.IsPreorder != true) ||
                (availabilityConditions.Contains("restocked") && p.IsReStock == true) ||
                (availabilityConditions.Contains("soldout") && p.Stock == 0)
            );
        }

        // Csak FunkoPop-nál: isOnSale filter
        if (filter.IsOnSale.HasValue)
            funkoQuery = funkoQuery.Where(p => p.IsOnSale == filter.IsOnSale.Value);

        if (filter.IsNew.HasValue)
            funkoQuery = funkoQuery.Where(p => p.IsNew == filter.IsNew.Value);
        if (filter.IsLimitedEdition.HasValue)
            funkoQuery = funkoQuery.Where(p => p.IsLimitedEdition == filter.IsLimitedEdition.Value);
        if (filter.IsExclusive.HasValue)
            funkoQuery = funkoQuery.Where(p => p.IsExclusive == filter.IsExclusive.Value);
        if (filter.IsChase.HasValue)
            funkoQuery = funkoQuery.Where(p => p.IsChase == filter.IsChase.Value);
        if (filter.IsSpecial.HasValue)
            funkoQuery = funkoQuery.Where(p => p.IsSpecial == filter.IsSpecial.Value);

        if (filter.Badges != null && filter.Badges.Any())
            funkoQuery = funkoQuery.Where(p => p.Badges.Any(b => filter.Badges.Contains(Convert.ToInt32(b.Badge))));

        if (filter.Tags != null && filter.Tags.Any())
            funkoQuery = funkoQuery.Where(p => p.FunkoPopTags != null && p.FunkoPopTags.Any(t => filter.Tags.Contains(Convert.ToInt32(t.Name))));

        // Labubu szűrés (mostantól van isOnSale filter!)
        var labubuQuery = _context.Labubus.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            labubuQuery = labubuQuery.Where(p => p.Name.Contains(filter.SearchTerm) ||
                                                 (p.Description != null && p.Description.Contains(filter.SearchTerm)));

        if (filter.PriceRange != null)
        {
            var min = GetDecimalValue(filter.PriceRange, "Min");
            var max = GetDecimalValue(filter.PriceRange, "Max");
            if (min.HasValue)
                labubuQuery = labubuQuery.Where(p => p.Price >= min.Value);
            if (max.HasValue)
                labubuQuery = labubuQuery.Where(p => p.Price <= max.Value);
        }

        if (filter.Availability != null && filter.Availability.Any())
        {
            var availabilityConditions = filter.Availability.Select(av => av.ToLower()).ToList();
            labubuQuery = labubuQuery.Where(p =>
                (availabilityConditions.Contains("instock") && p.Stock > 0) ||
                (availabilityConditions.Contains("preorder") && p.IsPreorder == true) ||
                (availabilityConditions.Contains("outofstock") && p.Stock == 0 && p.IsPreorder != true) ||
                (availabilityConditions.Contains("restocked") && p.IsReStock == true) ||
                (availabilityConditions.Contains("soldout") && p.Stock == 0)
            );
        }

        if (filter.IsOnSale.HasValue)
            labubuQuery = labubuQuery.Where(p => p.IsOnSale == filter.IsOnSale.Value);

        if (filter.IsNew.HasValue)
            labubuQuery = labubuQuery.Where(p => p.IsNew == filter.IsNew.Value);
        if (filter.IsLimitedEdition.HasValue)
            labubuQuery = labubuQuery.Where(p => p.IsLimitedEdition == filter.IsLimitedEdition.Value);

        // Típus szűrés
        var funkoEnabled = filter.Types == null || (filter.Types.Count > 0 && filter.Types.Contains(Convert.ToInt32(ProductType.FunkoPop)));
        var labubuEnabled = filter.Types == null || (filter.Types.Count > 0 && filter.Types.Contains(Convert.ToInt32(ProductType.Labubu)));

        var funkoList = funkoEnabled
            ? await funkoQuery.Include(f => f.Badges).Include(f => f.FunkoPopTags).ToListAsync()
            : new List<FunkoPop>();

        var labubuList = labubuEnabled
            ? await labubuQuery.ToListAsync()
            : new List<Labubu>();

        var dtos = new List<ProductDto>();

        dtos.AddRange(funkoList.Select(f =>
        {
            var dto = MapFunkoPopDto(f);
            dto.IsFavorite = f.Id != Guid.Empty && favoriteFunkoIds.Contains(f.Id);
            dto.ProductType = ProductType.FunkoPop;
            return dto;
        }));

        dtos.AddRange(labubuList.Select(l =>
        {
            var dto = MapLabubuDto(l);
            dto.IsFavorite = l.Id != Guid.Empty && favoriteLabubuIds.Contains(l.Id);
            dto.ProductType = ProductType.Labubu;
            return dto;
        }));

        // Rendezés
        switch (filter.SortBy)
        {
            case ProductSortBy.BestMatch:
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    dtos = dtos
                        .Select(p => new
                        {
                            Product = p,
                            Score = CalculateRelevanceScore(p, filter.SearchTerm)
                        })
                        .OrderByDescending(x => x.Score)
                        .ThenByDescending(x => x.Product.CreatedAt)
                        .Select(x => x.Product)
                        .ToList();
                }
                else
                {
                    dtos = dtos.OrderByDescending(p => p.CreatedAt).ToList();
                }
                break;

            case ProductSortBy.Popular:
                dtos = await ApplyPopularitySorting(dtos);
                break;

            case ProductSortBy.PriceAsc:
                dtos = dtos.OrderBy(p => p.Price).ToList();
                break;

            case ProductSortBy.PriceDesc:
                dtos = dtos.OrderByDescending(p => p.Price).ToList();
                break;

            case ProductSortBy.Newest:
            default:
                dtos = dtos.OrderByDescending(p => p.CreatedAt).ToList();
                break;
        }

        // SortDirection alkalmazása
        if (filter.SortDirection == SortDirection.Asc &&
            (filter.SortBy == ProductSortBy.PriceDesc || filter.SortBy == ProductSortBy.Newest))
        {
            dtos = dtos.Reverse<ProductDto>().ToList();
        }

        var totalItems = dtos.Count;
        var pagedDtos = dtos
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            items = pagedDtos,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            },
            enums = new EnumListsDto
            {
                Categories = _enumService.GetFunkoCategoryList() ?? new List<EnumValueDto>(),
                Franchises = _enumService.GetFranchiseList() ?? new List<EnumValueDto>(),
                Badges = _enumService.GetBadgeList() ?? new List<EnumValueDto>(),
                FunkoPopTagTypes = _enumService.GetFunkoPopTagTypeList() ?? new List<EnumValueDto>()
            }
        });
    }

    // Segédfüggvény a PriceRange objektumhoz
    private static decimal? GetDecimalValue(object priceRange, string propertyName)
    {
        var prop = priceRange.GetType().GetProperty(propertyName);
        if (prop == null) return null;
        var value = prop.GetValue(priceRange);
        if (value == null) return null;
        if (value is decimal dec) return dec;
        if (decimal.TryParse(value.ToString(), out var result)) return result;
        return null;
    }

    // Relevancia pontszám (egyszerűsített)
    private static int CalculateRelevanceScore(ProductDto product, string searchTerm)
    {
        var score = 0;
        var lowerSearchTerm = searchTerm.ToLower();
        var lowerName = product.Name?.ToLower() ?? "";
        var lowerDescription = product.Description?.ToLower() ?? "";

        if (lowerName.Equals(lowerSearchTerm))
            score += 100;
        else if (lowerName.Contains(lowerSearchTerm))
            score += 50;
        if (lowerDescription.Contains(lowerSearchTerm))
            score += 20;
        if (lowerName.StartsWith(lowerSearchTerm))
            score += 30;

        return score;
    }

    // Népszerűség rendezés (egyszerűsített)
    private async Task<List<ProductDto>> ApplyPopularitySorting(List<ProductDto> dtos)
    {
        var productIds = dtos.Where(p => p.Id != null).Select(p => p.Id.Value).ToList();

        var orderCounts = await _context.OrderItems
            .Where(oi => productIds.Contains(oi.ProductId))
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId, x => x.Count);

        var favoriteCounts = await _context.LikedItems
            .Where(li => productIds.Contains(li.ProductId))
            .GroupBy(li => li.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId, x => x.Count);

        return dtos
            .Select(p => new
            {
                Product = p,
                Popularity = (orderCounts.TryGetValue(p.Id.Value, out var oc) ? oc * 3 : 0) +
                             (favoriteCounts.TryGetValue(p.Id.Value, out var fc) ? fc : 0)
            })
            .OrderByDescending(x => x.Popularity)
            .ThenByDescending(x => x.Product.CreatedAt)
            .Select(x => x.Product)
            .ToList();
    }
    //[HttpGet("search")]
    //public async Task<ActionResult> SearchProducts(
    //   [FromQuery] ProductFilterDto filter,
    //   [FromQuery] int page = 1,
    //   [FromQuery] int pageSize = 20)
    //{
    //    var userId = await GetBackendUserIdAsync();
    //    var (favoriteFunkoIds, favoriteLabubuIds) = await GetUserFavoriteIdsAsync(userId);

    //    // FunkoPop szűrés
    //    var funkoQuery = _context.FunkoPops.AsQueryable();
    //    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
    //        funkoQuery = funkoQuery.Where(p => p.Name.Contains(filter.SearchTerm));
    //    if (filter.Categories != null && filter.Categories.Any())
    //        funkoQuery = funkoQuery.Where(p => filter.Categories.Contains(p.Category));
    //    if (filter.Franchises != null && filter.Franchises.Any())
    //        funkoQuery = funkoQuery.Where(p => filter.Franchises.Contains(p.Franchise ?? Franchise.Other));
    //    if (filter.PriceRange != null)
    //        funkoQuery = funkoQuery.Where(p => p.Price >= filter.PriceRange.Min && p.Price <= filter.PriceRange.Max);
    //    if (filter.Availability != null && filter.Availability.Any())
    //    {
    //        var avail = filter.Availability;
    //        funkoQuery = funkoQuery.Where(p =>
    //            (avail.Contains("inStock") && p.Stock > 0) ||
    //            (avail.Contains("preorder") && p.IsPreorder) ||
    //            (avail.Contains("outOfStock") && p.Stock == 0)
    //        );
    //    }
    //    if (filter.IsOnSale == true)
    //        funkoQuery = funkoQuery.Where(p => p.IsOnSale);
    //    if (filter.IsNew == true)
    //        funkoQuery = funkoQuery.Where(p => p.IsNew);
    //    if (filter.IsLimitedEdition == true)
    //        funkoQuery = funkoQuery.Where(p => p.IsLimitedEdition);
    //    if (filter.IsExclusive == true)
    //        funkoQuery = funkoQuery.Where(p => p.IsExclusive);
    //    if (filter.IsChase == true)
    //        funkoQuery = funkoQuery.Where(p => p.IsChase);

    //    // Labubu szűrés
    //    var labubuQuery = _context.Labubus.AsQueryable();
    //    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
    //        labubuQuery = labubuQuery.Where(p => p.Name.Contains(filter.SearchTerm));
    //    if (filter.PriceRange != null)
    //        labubuQuery = labubuQuery.Where(p => p.Price >= filter.PriceRange.Min && p.Price <= filter.PriceRange.Max);
    //    if (filter.Availability != null && filter.Availability.Any())
    //    {
    //        var avail = filter.Availability;
    //        labubuQuery = labubuQuery.Where(p =>
    //            (avail.Contains("inStock") && p.Stock > 0) ||
    //            (avail.Contains("preorder") && p.IsPreorder) ||
    //            (avail.Contains("outOfStock") && p.Stock == 0)
    //        );
    //    }
    //    if (filter.IsOnSale == true)
    //        labubuQuery = labubuQuery.Where(p => p.IsOnSale);
    //    if (filter.IsNew == true)
    //        labubuQuery = labubuQuery.Where(p => p.IsNew);
    //    if (filter.IsLimitedEdition == true)
    //        labubuQuery = labubuQuery.Where(p => p.IsLimitedEdition);

    //    // Típus szűrés
    //    var funkoEnabled = filter.Types == null || filter.Types.Contains(ProductType.FunkoPop);
    //    var labubuEnabled = filter.Types == null || filter.Types.Contains(ProductType.Labubu);

    //    var funkoList = funkoEnabled
    //        ? await funkoQuery.ToListAsync()
    //        : new List<FunkoPop>();

    //    var labubuList = labubuEnabled
    //        ? await labubuQuery.ToListAsync()
    //        : new List<Labubu>();

    //    // DTO mapping + IsFavorite
    //    var dtos = new List<ProductDto>();
    //    dtos.AddRange(funkoList.Select(f =>
    //    {
    //        var dto = MapFunkoPopDto(f);
    //        dto.IsFavorite = f.Id != Guid.Empty && favoriteFunkoIds.Contains(f.Id);
    //        return dto;
    //    }));
    //    dtos.AddRange(labubuList.Select(l =>
    //    {
    //        var dto = MapLabubuDto(l);
    //        dto.IsFavorite = l.Id != Guid.Empty && favoriteLabubuIds.Contains(l.Id);
    //        return dto;
    //    }));

    //    // Rendezés
    //    switch (filter.SortBy)
    //    {
    //        case ProductSortBy.BestMatch:
    //            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
    //            {
    //                dtos = dtos
    //                    .Select(p => new
    //                    {
    //                        Product = p,
    //                        Score =
    //                            (p.Name != null && p.Name.Equals(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ? 100 : 0) +
    //                            (p.Name != null && p.Name.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ? 50 : 0) +
    //                            (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ? 20 : 0)
    //                    })
    //                    .OrderByDescending(x => x.Score)
    //                    .ThenByDescending(x => x.Product.CreatedAt)
    //                    .Select(x => x.Product)
    //                    .ToList();
    //            }
    //            else
    //            {
    //                dtos = dtos.OrderByDescending(p => p.CreatedAt).ToList();
    //            }
    //            break;

    //        case ProductSortBy.Popular:
    //            var orderCounts = await _context.OrderItems
    //                .GroupBy(oi => oi.ProductId)
    //                .Select(g => new { ProductId = g.Key, Count = g.Count() })
    //                .ToDictionaryAsync(x => x.ProductId, x => x.Count);

    //            var favoriteCounts = await _context.LikedItems
    //                .GroupBy(li => li.ProductId)
    //                .Select(g => new { ProductId = g.Key, Count = g.Count() })
    //                .ToDictionaryAsync(x => x.ProductId, x => x.Count);

    //            dtos = dtos
    //                .Select(p => new
    //                {
    //                    Product = p,
    //                    Popularity =
    //                        (p.Id.HasValue && orderCounts.TryGetValue(p.Id.Value, out var oc) ? oc : 0) +
    //                        (p.Id.HasValue && favoriteCounts.TryGetValue(p.Id.Value, out var fc) ? fc : 0)
    //                })
    //                .OrderByDescending(x => x.Popularity)
    //                .ThenByDescending(x => x.Product.CreatedAt)
    //                .Select(x => x.Product)
    //                .ToList();
    //            break;

    //        case ProductSortBy.PriceAsc:
    //            dtos = dtos.OrderBy(p => p.Price).ToList();
    //            break;

    //        case ProductSortBy.PriceDesc:
    //            dtos = dtos.OrderByDescending(p => p.Price).ToList();
    //            break;

    //        case ProductSortBy.Newest:
    //        default:
    //            dtos = dtos.OrderByDescending(p => p.CreatedAt).ToList();
    //            break;
    //    }

    //    // SortDirection alkalmazása (ha nem relevancia vagy popular)
    //    if (filter.SortDirection.HasValue && filter.SortBy != ProductSortBy.BestMatch && filter.SortBy != ProductSortBy.Popular)
    //    {
    //        if (filter.SortDirection == SortDirection.Asc)
    //            dtos = dtos.Reverse<ProductDto>().ToList();
    //        // Desc alapból rendezve van
    //    }

    //    var totalItems = dtos.Count;
    //    var pagedDtos = dtos
    //        .Skip((page - 1) * pageSize)
    //        .Take(pageSize)
    //        .ToList();

    //    return Ok(new
    //    {
    //        items = pagedDtos,
    //        pagination = new
    //        {
    //            currentPage = page,
    //            pageSize,
    //            totalItems
    //        },
    //        enums = new EnumListsDto
    //        {
    //            Categories = _enumService.GetFunkoCategoryList() ?? new List<EnumValueDto>(),
    //            Franchises = _enumService.GetFranchiseList() ?? new List<EnumValueDto>(),
    //            Badges = _enumService.GetBadgeList() ?? new List<EnumValueDto>(),
    //            FunkoPopTagTypes = _enumService.GetFunkoPopTagTypeList() ?? new List<EnumValueDto>()
    //        }
    //    });
    //}

    [HttpGet("preorders")]
    public async Task<ActionResult<IEnumerable<PromotionItemDto>>> GetAllPreorders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = await GetBackendUserIdAsync();
        var (favoriteFunkoIds, favoriteLabubuIds) = await GetUserFavoriteIdsAsync(userId);

        var funkos = await _context.FunkoPops
            .AsNoTracking()
            .Where(f => f.IsPreorder)
            .ToListAsync();

        var labubus = await _context.Labubus
            .AsNoTracking()
            .Where(l => l.IsPreorder)
            .ToListAsync();

        var funkoDtos = funkos.Select(f =>
        {
            var dto = MapFunkoPopDto(f);
            dto.IsFavorite = favoriteFunkoIds.Contains(f.Id);
            return dto;
        });

        var labubuDtos = labubus.Select(l =>
        {
            var dto = MapLabubuDto(l);
            dto.IsFavorite = favoriteLabubuIds.Contains(l.Id);
            return dto;
        });

        var all = funkoDtos
            .Select(dto => new PromotionItemDto
            {
                Type = ProductType.FunkoPop,
                FunkoPop = dto,
                Labubu = null
            })
            .Concat(labubuDtos.Select(dto => new PromotionItemDto
            {
                Type = ProductType.Labubu,
                FunkoPop = null,
                Labubu = dto
            }))
            .OrderByDescending(x => x.FunkoPop?.CreatedAt ?? x.Labubu?.CreatedAt);

        var paged = Paginate(all, page, pageSize).ToList();

        return Ok(paged);
    }

    [HttpGet("discounted")]
    public async Task<ActionResult<IEnumerable<PromotionItemDto>>> GetAllDiscounted([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = await GetBackendUserIdAsync();
        var (favoriteFunkoIds, favoriteLabubuIds) = await GetUserFavoriteIdsAsync(userId);

        var funkos = await _context.FunkoPops
            .AsNoTracking()
            .Where(f => f.IsOnSale)
            .ToListAsync();

        var labubus = await _context.Labubus
            .AsNoTracking()
            .Where(l => l.IsOnSale)
            .ToListAsync();

        var funkoDtos = funkos.Select(f =>
        {
            var dto = MapFunkoPopDto(f);
            dto.IsFavorite = favoriteFunkoIds.Contains(f.Id);
            return dto;
        });

        var labubuDtos = labubus.Select(l =>
        {
            var dto = MapLabubuDto(l);
            dto.IsFavorite = favoriteLabubuIds.Contains(l.Id);
            return dto;
        });

        var all = funkoDtos
            .Select(dto => new PromotionItemDto
            {
                Type = ProductType.FunkoPop,
                FunkoPop = dto,
                Labubu = null
            })
            .Concat(labubuDtos.Select(dto => new PromotionItemDto
            {
                Type = ProductType.Labubu,
                FunkoPop = null,
                Labubu = dto
            }))
            .OrderByDescending(x => x.FunkoPop?.CreatedAt ?? x.Labubu?.CreatedAt);

        var paged = Paginate(all, page, pageSize).ToList();

        return Ok(paged);
    }

    [HttpGet("latest-discounted")]
    public async Task<ActionResult<IEnumerable<PromotionItemDto>>> GetLatestDiscounted()
    {
        var userId = await GetBackendUserIdAsync();
        var (favoriteFunkoIds, favoriteLabubuIds) = await GetUserFavoriteIdsAsync(userId);

        var funkos = await _context.FunkoPops
            .AsNoTracking()
            .Where(f => f.IsOnSale)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var labubus = await _context.Labubus
            .AsNoTracking()
            .Where(l => l.IsOnSale)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var funkoDtos = funkos.Select(f =>
        {
            var dto = MapFunkoPopDto(f);
            dto.IsFavorite = favoriteFunkoIds.Contains(f.Id);
            return dto;
        });

        var labubuDtos = labubus.Select(l =>
        {
            var dto = MapLabubuDto(l);
            dto.IsFavorite = favoriteLabubuIds.Contains(l.Id);
            return dto;
        });

        var all = funkoDtos
            .Select(dto => new PromotionItemDto
            {
                Type = ProductType.FunkoPop,
                FunkoPop = dto,
                Labubu = null
            })
            .Concat(labubuDtos.Select(dto => new PromotionItemDto
            {
                Type = ProductType.Labubu,
                FunkoPop = null,
                Labubu = dto
            }))
            .OrderByDescending(x => x.FunkoPop?.CreatedAt ?? x.Labubu?.CreatedAt)
            .Take(10)
            .ToList();

        return Ok(all);
    }

    // 2. Előrendelhető termékek (5 legújabb FunkoPop vagy Labubu, ahol IsPreorder)
    [HttpGet("latest-preorders")]
    public async Task<ActionResult<IEnumerable<PromotionItemDto>>> GetLatestPreorders()
    {
        var funkos = await _context.FunkoPops
            .AsNoTracking()
            .Where(f => f.IsPreorder)
            .ToListAsync();

        var labubus = await _context.Labubus
            .AsNoTracking()
            .Where(l => l.IsPreorder)
            .ToListAsync();

        var all = funkos
            .Select(f => new PromotionItemDto
            {
                Type = ProductType.FunkoPop,
                FunkoPop = MapFunkoPopDto(f),
                Labubu = null
            })
            .Concat(labubus.Select(l => new PromotionItemDto
            {
                Type = ProductType.Labubu,
                FunkoPop = null,
                Labubu = MapLabubuDto(l)
            }))
            .OrderByDescending(x => x.FunkoPop?.CreatedAt ?? x.Labubu?.CreatedAt)
            .Take(5)
            .ToList();

        return Ok(all);
    }

    // 3. Kedvencek (bejelentkezett user alapján, 5 legújabb kedvenc FunkoPop vagy Labubu)
    [HttpGet("latest-favorites")]
    public async Task<ActionResult<IEnumerable<PromotionItemDto>>> GetLatestFavorites([FromQuery] Guid userId)
    {
        var likedItems = await _context.LikedItems
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Id)
            .Take(20)
            .ToListAsync();

        var funkoIds = likedItems.Where(l => l.ProductType == ProductType.FunkoPop).Select(l => l.ProductId).ToList();
        var labubuIds = likedItems.Where(l => l.ProductType == ProductType.Labubu).Select(l => l.ProductId).ToList();

        var funkos = await _context.FunkoPops
            .AsNoTracking()
            .Where(f => funkoIds.Contains(f.Id))
            .ToListAsync();

        var labubus = await _context.Labubus
            .AsNoTracking()
            .Where(l => labubuIds.Contains(l.Id))
            .ToListAsync();

        var all = funkos
            .Select(f => new PromotionItemDto
            {
                Type = ProductType.FunkoPop,
                FunkoPop = MapFunkoPopDto(f),
                Labubu = null
            })
            .Concat(labubus.Select(l => new PromotionItemDto
            {
                Type = ProductType.Labubu,
                FunkoPop = null,
                Labubu = MapLabubuDto(l)
            }))
            .OrderByDescending(x => x.FunkoPop?.CreatedAt ?? x.Labubu?.CreatedAt)
            .Take(5)
            .ToList();

        return Ok(all);
    }

    // --- Mapping segédfüggvények ---
    private static FunkoPopDto MapFunkoPopDto(FunkoPop f) => new FunkoPopDto
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
        IsReStock = f.IsReStock,
        IsNew = f.IsNew,
        ReleaseDate = f.ReleaseDate,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt,
        IsActive = f.IsActive,
        IsOnSale = f.IsOnSale,
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

    private static LabubuDto MapLabubuDto(Labubu l) => new LabubuDto
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
        IsOnSale = l.IsOnSale,
        IsVisible = l.IsVisible,
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


    private string? GetFirebaseUid()
    {
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }
        // Próbáld ki, melyik claimben van a Firebase UID
        return User.FindFirst("user_id")?.Value
            ?? User.FindFirst("firebase_uid")?.Value
            ?? User.FindFirst("sub")?.Value;
    }

    private async Task<Guid?> GetBackendUserIdAsync()
    {
        var firebaseUid = GetFirebaseUid();
        if (string.IsNullOrEmpty(firebaseUid))
            return null;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

        return user?.Id;
    }


    private async Task<(HashSet<Guid> funkoIds, HashSet<Guid> labubuIds)> GetUserFavoriteIdsAsync(Guid? userId)
    {
        if (userId == null || userId == Guid.Empty)
            return (new HashSet<Guid>(), new HashSet<Guid>());

        var likedItems = await _context.LikedItems
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .ToListAsync();

        var funkoIds = likedItems
            .Where(l => l.ProductType == ProductType.FunkoPop)
            .Select(l => l.ProductId)
            .ToHashSet();

        var labubuIds = likedItems
            .Where(l => l.ProductType == ProductType.Labubu)
            .Select(l => l.ProductId)
            .ToHashSet();

        return (funkoIds, labubuIds);
    }

    private Guid? GetCurrentUserId()
    {
        // Próbáld meg Guid-ként értelmezni
        var userIdString = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        if (Guid.TryParse(userIdString, out var userId))
            return userId;

        // Ha nem Guid, akkor lehet Firebase UID
        var firebaseUid = userIdString;
        if (!string.IsNullOrEmpty(firebaseUid))
        {
            var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.FirebaseUid == firebaseUid);
            return user?.Id;
        }

        return null;
    }

    private async Task<(HashSet<Guid> funkoIds, HashSet<Guid> labubuIds)> GetUserFavoriteIds(Guid? userId)
    {
        if (userId == null || userId == Guid.Empty)
            return (new HashSet<Guid>(), new HashSet<Guid>());

        var likedItems = await _context.LikedItems
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .ToListAsync();

        var funkoIds = likedItems
            .Where(l => l.ProductType == ProductType.FunkoPop)
            .Select(l => l.ProductId)
            .ToHashSet();

        var labubuIds = likedItems
            .Where(l => l.ProductType == ProductType.Labubu)
            .Select(l => l.ProductId)
            .ToHashSet();

        return (funkoIds, labubuIds);
    }
    [HttpGet("recently-viewed")]
    public async Task<ActionResult<IEnumerable<PromotionItemDto>>> GetRecentlyViewed([FromQuery] Guid userId)
    {
        var recentlyViewedItems = await _context.RecentlyViewedItems
            .AsNoTracking()
            .Where(rv => rv.UserId == userId)
            .OrderByDescending(rv => rv.ViewedAt)
            .Take(5)
            .ToListAsync();

        var funkoIds = recentlyViewedItems
            .Where(rv => rv.ProductType == ProductType.FunkoPop)
            .Select(rv => rv.ProductId)
            .ToList();

        var labubuIds = recentlyViewedItems
            .Where(rv => rv.ProductType == ProductType.Labubu)
            .Select(rv => rv.ProductId)
            .ToList();

        var funkos = await _context.FunkoPops
            .AsNoTracking()
            .Where(f => funkoIds.Contains(f.Id))
            .ToListAsync();

        var labubus = await _context.Labubus
            .AsNoTracking()
            .Where(l => labubuIds.Contains(l.Id))
            .ToListAsync();

        var result = new List<PromotionItemDto>();
        foreach (var item in recentlyViewedItems)
        {
            if (item.ProductType == ProductType.FunkoPop)
            {
                var funko = funkos.FirstOrDefault(f => f.Id == item.ProductId);
                if (funko != null)
                {
                    result.Add(new PromotionItemDto
                    {
                        Type = ProductType.FunkoPop,
                        FunkoPop = MapFunkoPopDto(funko),
                        Labubu = null
                    });
                }
            }
            else if (item.ProductType == ProductType.Labubu)
            {
                var labubu = labubus.FirstOrDefault(l => l.Id == item.ProductId);
                if (labubu != null)
                {
                    result.Add(new PromotionItemDto
                    {
                        Type = ProductType.Labubu,
                        FunkoPop = null,
                        Labubu = MapLabubuDto(labubu)
                    });
                }
            }
        }

        return Ok(result);
    }


    [HttpGet("autocomplete")]
    public async Task<ActionResult> GetAutocompleteSuggestions([FromQuery] string query, [FromQuery] int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(new { suggestions = new List<string>() });

        var loweredQuery = query.ToLower();

        // FunkoPop nevek (SQL-ben kereshető)
        var funkoSuggestions = await _context.FunkoPops
            .AsNoTracking()
            .Where(f => !string.IsNullOrEmpty(f.Name) && f.Name.ToLower().Contains(loweredQuery))
            .Select(f => f.Name)
            .Distinct()
            .Take(maxResults)
            .ToListAsync();

        // Labubu nevek (SQL-ben kereshető)
        var labubuSuggestions = await _context.Labubus
            .AsNoTracking()
            .Where(l => !string.IsNullOrEmpty(l.Name) && l.Name.ToLower().Contains(loweredQuery))
            .Select(l => l.Name)
            .Distinct()
            .Take(maxResults)
            .ToListAsync();

        // FunkoPop kategóriák (enum ToString csak memóriában!)
        var funkoCategories = await _context.FunkoPops
            .AsNoTracking()
            .Select(f => f.Category)
            .Distinct()
            .ToListAsync();

        var categorySuggestions = funkoCategories
            .Select(c => c.ToString())
            .Where(c => c.ToLower().Contains(loweredQuery))
            .Distinct()
            .Take(maxResults)
            .ToList();

        // Labubu sorozat/edition (memóriában, mert lehet null)
        var labubuSeries = await _context.Labubus
            .AsNoTracking()
            .Select(l => l.Series)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToListAsync();

        var labubuEdition = await _context.Labubus
            .AsNoTracking()
            .Select(l => l.Edition)
            .Where(e => !string.IsNullOrEmpty(e))
            .Distinct()
            .ToListAsync();

        var seriesSuggestions = labubuSeries
            .Where(s => s != null && s.ToLower().Contains(loweredQuery))
            .Distinct()
            .Take(maxResults)
            .ToList();

        var editionSuggestions = labubuEdition
            .Where(e => e != null && e.ToLower().Contains(loweredQuery))
            .Distinct()
            .Take(maxResults)
            .ToList();

        // Összefésülés, duplikációk nélkül
        var suggestions = funkoSuggestions
            .Concat(labubuSuggestions)
            .Concat(categorySuggestions)
            .Concat(seriesSuggestions)
            .Concat(editionSuggestions)
            .Distinct()
            .Take(maxResults)
            .ToList();

        return Ok(new { suggestions });
    }
}