namespace Domain.Entities;

public class ProductStock
{
    public required string LotCode { get; set; }
    public required string WhmCode { get; set; }
    public required string Name { get; set; }
    public required string UnitOfMeasure { get; set; }
    public int Weight { get; set; }
    public int PackageSize { get; set; }
    public DateTime? ExpirationDate { get; set; }

    public decimal Quantity { get; set; }

    public decimal? Price { get; set; }

    public string? Barcode { get; set; }

    public string? MPBarcode { get; set; }

    public string? ImagePath { get; set; }

    public string? Comment { get; set; }

    public string? Image { get; set; }
}
