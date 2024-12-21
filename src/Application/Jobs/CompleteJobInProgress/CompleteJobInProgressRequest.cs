using Domain.Enums;

namespace Application.Jobs.CompleteJobInProgress;

public record CompleteJobInProgressRequest
{
    public JobCompletitionType CompletitionType { get; init; }
    public string Note { get; init; } = string.Empty;
}
