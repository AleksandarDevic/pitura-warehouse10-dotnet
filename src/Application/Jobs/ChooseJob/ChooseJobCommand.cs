using Application.Abstractions.Messaging;
using Domain.Models;

namespace Application.Jobs.ChooseJob;

public record ChooseJobCommand(long JobId) : ICommand<JobInProgressResponse>;