using kz_webshop_be.DTOs;
using kz_webshop_be.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class PromotionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PromotionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("active")]
    public async Task<ActionResult<PromotionDto>> GetActivePromotion()
    {
        var promo = await _context.Promotions
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefaultAsync();

        if (promo == null)
            return NotFound();

        var dto = new PromotionDto
        {
            Id = promo.Id,
            FreeShippingFrom = promo.FreeShippingFrom,
            PromotionText = promo.PromotionText,
            PromotionCode = promo.PromotionCode,
            DiscountPercent = promo.DiscountPercent,
            StartDate = promo.StartDate,
            EndDate = promo.EndDate
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<Promotion>> CreatePromotion([FromBody] Promotion promotion)
    {
        promotion.Id = Guid.NewGuid();
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPromotion), new { id = promotion.Id }, promotion);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Promotion>> GetPromotion(Guid id)
    {
        var promo = await _context.Promotions.FindAsync(id);
        if (promo == null)
            return NotFound();
        return Ok(promo);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePromotion(Guid id, [FromBody] Promotion promotion)
    {
        if (id != promotion.Id)
            return BadRequest();

        _context.Entry(promotion).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePromotion(Guid id)
    {
        var promo = await _context.Promotions.FindAsync(id);
        if (promo == null)
            return NotFound();

        _context.Promotions.Remove(promo);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}