using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.JobItems.CompleteJobItem;

public record CompleteJobItemCommand(long JobInProgressId, long JobItemId, double EnteredQuantity, JobItemStatus Status) : ICommand;
