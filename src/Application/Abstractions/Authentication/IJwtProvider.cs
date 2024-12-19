using Domain.Entities;
using Domain.Models;

namespace Application.Abstractions.Authentication;

public interface IJwtProvider
{
    JwtResponse Create(OperatorTerminal operatorTerminal);
}
