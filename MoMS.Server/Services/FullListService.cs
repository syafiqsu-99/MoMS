using MoMS.Server.Data;
using MoMS.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Services;

public class FullListService(MoMsDbContext context) : BaseService(context)
{
    public async Task<List<FullList>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await Context.FullList
            .AsNoTracking()
            .Where(f => f.SNum != null)
            .OrderBy(f => f.Item)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Location>> GetLocationsAsync(CancellationToken cancellationToken)
    {
        return await Context.Locations
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(FullListCreateDto dto, CancellationToken cancellationToken)
    {
        var entity = new FullList
        {
            Item = dto.Item,
            SNum = dto.SNum,
            Type = dto.Type,
            Rack = dto.Rack,
            Level = dto.Level,
            Status = dto.Status,
            Remark = dto.Remark,
            AccumUsage = 0,
            Usage = 0,
            PlanUsage = dto.PlanUsage,
            LastServ = DateTime.Now,
            PlanServ = dto.PlanServ,
            Repeat = dto.Repeat
        };

        Context.FullList.Add(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    // Returns false when the serial number does not exist so the controller
    // can translate that into a 404.
    public async Task<bool> UpdateAsync(string sNum, FullListUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await Context.FullList
            .FirstOrDefaultAsync(f => f.SNum == sNum, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.Item = dto.Item;
        entity.Rack = dto.Rack;
        entity.Level = dto.Level;
        entity.Location = dto.Location;
        entity.Status = dto.Status;
        entity.Remark = dto.Remark;
        entity.Usage = dto.Usage;
        entity.LastServ = dto.LastServ;

        await Context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string sNum, CancellationToken cancellationToken)
    {
        var rows = await Context.FullList
            .Where(f => f.SNum == sNum)
            .ExecuteDeleteAsync(cancellationToken);

        return rows > 0;
    }
}
