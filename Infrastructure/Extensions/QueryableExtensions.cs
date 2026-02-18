using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static async Task<(List<T> Items, int Total)> ToPagedListAsync<T>(
        this IQueryable<T> query, int pageNumber, int pageSize)
    {
        var total = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }
}
