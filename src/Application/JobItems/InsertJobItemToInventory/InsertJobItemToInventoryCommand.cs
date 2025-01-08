using Application.Abstractions.Messaging;
using Domain.Models;

namespace Application.JobItems.InsertJobItemToInventory;

public record InsertJobItemToInventoryCommand(long JobInProgressId, string ReadedField1, string ReadedField2, double ReadedField3)
    : ICommand<JobItemResponse>;
