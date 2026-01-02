using Microsoft.EntityFrameworkCore;

namespace kz_webshop_be.Services
{

    public class FunkoPopService
    {
        private readonly ApplicationDbContext _context;

        public FunkoPopService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<string>> SearchNamesAsync(string term, int maxResults = 10)
        {
            return await _context.FunkoPops
                .Where(f => !f.IsDeleted && f.Name.Contains(term))
                .OrderBy(f => f.Name)
                .Select(f => f.Name)
                .Take(maxResults)
                .ToListAsync();
        }
    }
}
