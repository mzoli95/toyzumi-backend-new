using Quartz;
using Microsoft.EntityFrameworkCore;

public class ProductActivationJob : IJob
{
    private readonly ApplicationDbContext _db;

    public ProductActivationJob(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var funkosToActivate = await _db.FunkoPops
      .Where(f => !f.IsActive && f.Stock > 0 && !f.IsPreorder)
      .ToListAsync();
        funkosToActivate.ForEach(f => f.IsActive = true);

        var labubusToActivate = await _db.Labubus
            .Where(l => !l.IsActive && l.Stock > 0 && !l.IsPreorder)
            .ToListAsync();
        labubusToActivate.ForEach(l => l.IsActive = true);

        if (funkosToActivate.Count > 0 || labubusToActivate.Count > 0)
            await _db.SaveChangesAsync();
    }
}