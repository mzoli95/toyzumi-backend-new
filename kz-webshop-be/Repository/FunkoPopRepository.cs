using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using Microsoft.EntityFrameworkCore;

namespace kz_webshop_be.Repository;

public class FunkoPopRepository : IFunkoPopRepository
{
    private readonly ApplicationDbContext _context;

    public FunkoPopRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FunkoPop>> GetAllAsync()
        => await _context.FunkoPops
            .Where(f => !f.IsDeleted)
            .Include(f => f.Images)
            .Include(f => f.Reviews)
            .Include(f => f.Comments)
            .Include(f => f.FunkoPopTags)
            .Include(f => f.RelatedProducts)
            .ToListAsync();

    public async Task<FunkoPop?> GetByIdAsync(Guid id)
        => await _context.FunkoPops
            .Where(f => !f.IsDeleted)
            .Include(f => f.Images)
            .Include(f => f.Reviews)
            .Include(f => f.Comments)
            .Include(f => f.FunkoPopTags)
            .Include(f => f.RelatedProducts)
            .FirstOrDefaultAsync(f => f.Id == id);

    public async Task AddAsync(FunkoPop funkoPop)
        => await _context.FunkoPops.AddAsync(funkoPop);

    public Task UpdateAsync(FunkoPop funkoPop)
    {
        _context.FunkoPops.Update(funkoPop);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FunkoPop funkoPop)
    {
        funkoPop.IsDeleted = true;
        _context.FunkoPops.Update(funkoPop);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}