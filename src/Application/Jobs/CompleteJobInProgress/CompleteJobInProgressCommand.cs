using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Jobs.CompleteJobInProgress;
public record CompleteJobInProgressCommand(long JobInProgressId, JobCompletitionType CompletitionType, string Note) : ICommand;
