using kz_webshop_be.Models;

namespace kz_webshop_be.Interfaces;

public interface ILabubuRepository
{
    Task<IEnumerable<Labubu>> GetAllAsync();
    Task<Labubu?> GetByIdAsync(Guid id);
    Task AddAsync(Labubu labubu);
    Task UpdateAsync(Labubu labubu);
    Task DeleteAsync(Labubu labubu);
    Task SaveChangesAsync();
}