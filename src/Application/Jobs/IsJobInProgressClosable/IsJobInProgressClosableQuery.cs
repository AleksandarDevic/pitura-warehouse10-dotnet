using Application.Abstractions.Messaging;

namespace Application.Jobs.IsJobInProgressClosable;
public record IsJobInProgressClosableQuery(long JobInProgressId) : IQuery<IsJobInProgressClosableQueryResult>;

