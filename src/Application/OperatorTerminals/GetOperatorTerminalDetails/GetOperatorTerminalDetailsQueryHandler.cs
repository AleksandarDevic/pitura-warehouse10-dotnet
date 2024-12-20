using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.OperatorTerminals.GetOperatorTerminalDetails;

internal sealed class GetOperatorTerminalDetailsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    : IQueryHandler<GetOperatorTerminalDetailsQuery, OperatorTerminalResponse>
{
    public async Task<Result<OperatorTerminalResponse>> Handle(GetOperatorTerminalDetailsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<OperatorTerminal> query = dbContext.OperatorTerminalSessions
            .AsNoTracking()
            .Where(x => x.Id == currentUserService.OperatorTerminalId);

        var operatorTerminalResponseQuery = query.Select(ot => new OperatorTerminalResponse
        {
            Id = ot.Id,
            OperatorId = ot.OperatorId,
            Operator = new OperatorResponse
            {
                Id = ot.Operator.Id,
                Name = ot.Operator.Name,
                IsActive = ot.Operator.IsActive
            },
            Terminal = new TerminalResponse
            {
                Id = ot.Terminal.Id,
                Name = ot.Operator.Name,
                IsActive = ot.Operator.IsActive
            },
            TerminalId = ot.TerminalId,
        });

        var result = await operatorTerminalResponseQuery.FirstOrDefaultAsync(cancellationToken);

        return result;
    }
}