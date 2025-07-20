using kz_webshop_be.Interfaces;
using kz_webshop_be.Models;
using Microsoft.EntityFrameworkCore;

namespace kz_webshop_be.Repository;

public class LabubuRepository : ILabubuRepository
{
    private readonly ApplicationDbContext _context;

    public LabubuRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Labubu>> GetAllAsync()
        => await _context.Labubus
            .Include(l => l.Images)
            .Include(l => l.Reviews)
            .Include(l => l.Comments)
            .Include(l => l.RelatedProducts)
            .ToListAsync();

    public async Task<Labubu?> GetByIdAsync(Guid id)
        => await _context.Labubus
            .Include(l => l.Images)
            .Include(l => l.Reviews)
            .Include(l => l.Comments)
            .Include(l => l.RelatedProducts)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task AddAsync(Labubu labubu)
        => await _context.Labubus.AddAsync(labubu);

    public Task UpdateAsync(Labubu labubu)
    {
        _context.Labubus.Update(labubu);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Labubu labubu)
    {
        _context.Labubus.Remove(labubu);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}