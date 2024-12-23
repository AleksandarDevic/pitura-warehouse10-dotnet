using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.JobItems.CompleteJobItem;

public record CompleteJobItemCommand(long JobInProgressId, long JobItemId, string RequiredFieldRead1, string RequiredFieldRead2, double RequiredFieldRead3, JobItemStatus Status) : ICommand;
