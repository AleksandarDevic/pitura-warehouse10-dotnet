using Domain.Enums;

namespace Domain.Models;

public record JobInProgressResponse
{
    public required int Id { get; set; }

    public required long JobId { get; set; }
    public required JobResponse Job { get; set; }

    public required int OperatorTerminalId { get; set; }

    public required DateTime StartDateTime { get; set; }
    public required DateTime? EndDateTime { get; set; }

    public required JobCompletitionType CompletionType { get; set; }
    public required string? Note { get; set; }
}
