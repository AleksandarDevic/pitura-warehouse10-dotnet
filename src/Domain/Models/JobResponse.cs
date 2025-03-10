using System.Text.RegularExpressions;
using Domain.Enums;

namespace Domain.Models;

public class JobResponse
{
    public required long Id { get; set; }
    public required string? Description { get; set; }
    public string? DescriptionShort => GetDescriptionShort(this.Description);
    public required short? AssignedOperatorId { get; set; }
    public required JobType Type { get; set; }
    public required DateTime CreationDateTime { get; set; }
    public required DateTime? DueDateTime { get; set; }
    public required JobCompletitionType CompletionType { get; set; }
    public required string? LastNote { get; set; }
    public required int? InventoryId { get; set; }
    public required bool IsVerified { get; set; }
    public required string? Client { get; set; }

    private static string? GetDescriptionShort(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var match = Regex.Match(description, @"^\d+/\d+/\d+/\d+");

        return match.Success ? match.Value : string.Empty;
    }
}
