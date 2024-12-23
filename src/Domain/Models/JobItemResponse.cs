using Domain.Enums;

namespace Domain.Models;

public record JobItemResponse
{
    public required long Id { get; set; }

    public required long JobId { get; set; }

    public required string? ItemDescription { get; set; }
    public string? ItemDescriptionShort => GetJobItemDescriptionShort(this.ItemDescription);
    public required string? RequiredField1 { get; set; }
    public required string? RequiredField2 { get; set; }
    public required double? RequiredField3 { get; set; }

    public required int? JobInProgressId { get; set; }

    public required double? ReadedField3 { get; set; }
    public required JobItemStatus ItemStatus { get; set; }

    private static string? GetJobItemDescriptionShort(string? itemDescription)
    {
        if (itemDescription is not null && itemDescription.Contains(','))
        {
            var commaIndex = itemDescription.IndexOf(',');
            return itemDescription[(commaIndex + 1)..].Trim();
        }
        return itemDescription;
    }
}
