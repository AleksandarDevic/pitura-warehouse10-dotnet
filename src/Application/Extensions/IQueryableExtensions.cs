using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Extensions;


public static class IQueryableExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        int number = pageNumber;
        int size = pageSize > 100 ? 100 : pageSize;

        number = Math.Max(number, 1);
        size = Math.Max(size, 1);

        int totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((number - 1) * size).Take(size).ToListAsync(cancellationToken);

        return new PagedList<T>(items, number, size, totalCount);
    }
}

