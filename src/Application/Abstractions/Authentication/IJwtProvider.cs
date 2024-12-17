using Application.Login;

namespace Application.Abstractions.Authentication;

public interface IJwtProvider
{
    JwtResponse Create(int operatorId, int terminalId);
}
