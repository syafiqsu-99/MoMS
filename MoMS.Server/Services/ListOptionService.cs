using MoMS.Server.Data;
using MoMS.Server.Models.Dtos;
using MoMS.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Services;

public class ListOptionService(MoMsDbContext context) : BaseService(context)
{
    private static readonly HashSet<string> AllowedCategories =
    [
        "production", "vendor", "mould_maker", "prepared", "purpose", "rack"
    ];

    public async Task<Dictionary<string, List<string>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rows = await Context.ListOptions
            .AsNoTracking()
            .OrderBy(o => o.Category)
            .ThenBy(o => o.SortOrder)
            .Select(o => new { o.Category, o.Value })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.Category)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Value).ToList());
    }

    public async Task<List<string>?> GetByCategoryAsync(string category, CancellationToken cancellationToken)
    {
        if (!AllowedCategories.Contains(category))
        {
            return null;
        }

        return await Context.ListOptions
            .AsNoTracking()
            .Where(o => o.Category == category)
            .OrderBy(o => o.SortOrder)
            .Select(o => o.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CreateAsync(ListOptionCreateDto dto, CancellationToken cancellationToken)
    {
        if (!AllowedCategories.Contains(dto.Category) || string.IsNullOrWhiteSpace(dto.Value))
        {
            return false;
        }

        var exists = await Context.ListOptions
            .AnyAsync(o => o.Category == dto.Category && o.Value == dto.Value, cancellationToken);

        if (exists)
        {
            return false;
        }

        var nextOrder = await Context.ListOptions
            .Where(o => o.Category == dto.Category)
            .Select(o => (int?)o.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        Context.ListOptions.Add(new ListOption
        {
            Category = dto.Category,
            Value = dto.Value.Trim(),
            SortOrder = nextOrder + 1
        });

        await Context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(ListOptionUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!AllowedCategories.Contains(dto.Category) || string.IsNullOrWhiteSpace(dto.NewValue))
        {
            return false;
        }

        var entity = await Context.ListOptions
            .FirstOrDefaultAsync(
                o => o.Category == dto.Category && o.Value == dto.OldValue,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.Value = dto.NewValue.Trim();
        await Context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(ListOptionDeleteDto dto, CancellationToken cancellationToken)
    {
        if (!AllowedCategories.Contains(dto.Category))
        {
            return false;
        }

        var rows = await Context.ListOptions
            .Where(o => o.Category == dto.Category && o.Value == dto.Value)
            .ExecuteDeleteAsync(cancellationToken);

        return rows > 0;
    }
}