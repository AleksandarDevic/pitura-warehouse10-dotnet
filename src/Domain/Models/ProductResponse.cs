namespace Domain.Models;

public record ProductResponse
{
    public string ProductDescription { get; init; } = null!;
    public string ProductName => GetProductName(ProductDescription);
    public string WhmCode { get; init; } = null!;
    public string ProductCode { get; init; } = null!;
    public List<string> ProductLotNumbers { get; init; } = [];

    private static string GetProductName(string itemDescription)
    {
        if (itemDescription is not null && itemDescription.Contains(','))
        {
            var commaIndex = itemDescription.IndexOf(',');
            return itemDescription[(commaIndex + 1)..].Trim();
        }
        return itemDescription ?? string.Empty;
    }
}
