using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.OperatorTerminals.GetActiveTerminals;

internal sealed class GetActiveTerminalsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetActiveTerminalsQuery, List<TerminalResponse>>
{
    public async Task<Result<List<TerminalResponse>>> Handle(GetActiveTerminalsQuery request, CancellationToken cancellationToken)
    {
        var result = await dbContext.Terminals
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => new TerminalResponse
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            })
        .ToListAsync(cancellationToken);

        return result;
    }
}
