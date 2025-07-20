using kz_webshop_be.Models;

namespace kz_webshop_be.Interfaces;

public interface IFunkoPopRepository
{
    Task<IEnumerable<FunkoPop>> GetAllAsync();
    Task<FunkoPop?> GetByIdAsync(Guid id);
    Task AddAsync(FunkoPop funkoPop);
    Task UpdateAsync(FunkoPop funkoPop);
    Task DeleteAsync(FunkoPop funkoPop);
    Task SaveChangesAsync();
}