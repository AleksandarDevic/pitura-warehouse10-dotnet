using Application.Abstractions.Messaging;
using Domain.Models;

namespace Application.Jobs.GetAssignedJob;

public record GetAssignedJobQuery : IQuery<JobResponse>;
