using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.TokenRefresh;

internal sealed class TokenRefreshCommandHandler(IApplicationDbContext dbContext, IJwtProvider jwtProvider) : ICommandHandler<TokenRefreshCommand, JwtResponse>
{
    public async Task<Result<JwtResponse>> Handle(TokenRefreshCommand request, CancellationToken cancellationToken)
    {
        var operatorTerminal = await dbContext.OperatorTerminalSessions.FirstOrDefaultAsync(x => x.Id == request.OperatorTerminalId, cancellationToken);
        if (operatorTerminal is null)
            return Result.Failure<JwtResponse>(OperatorTerminalErrors.NotFound);

        var result = jwtProvider.Create(operatorTerminal);

        return result;
    }
}

