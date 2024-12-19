using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Logout;

internal sealed class LogoutCommandHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var operatorTerminal = await dbContext.OperatorTerminalSessions.FirstOrDefaultAsync(x => x.Id == request.OperatorTerminalId, cancellationToken);
        if (operatorTerminal is null)
            return Result.Failure<Result>(OperatorTerminalErrors.NotFound);

        operatorTerminal.LogoutDateTime = dateTimeProvider.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
