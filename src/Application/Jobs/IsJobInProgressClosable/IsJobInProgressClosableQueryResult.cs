namespace Application.Jobs.IsJobInProgressClosable;

public record IsJobInProgressClosableQueryResult
{
    public bool IsClosable { get; init; }
    public int ItemsReadWithRequestedQuantity { get; init; }
    public int TotalItems { get; init; }
}
