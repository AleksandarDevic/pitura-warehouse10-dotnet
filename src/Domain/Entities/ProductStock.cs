namespace Domain.Entities;

public class ProductStock
{
    public required string ProductCodeLot { get; set; }
    public required string WhmCode { get; set; }
    public required string Name { get; set; }
    public required string UnitOfMeasure { get; set; }
    public double Weight { get; set; }
    public double PackageSize { get; set; }
    public DateTime? ExpirationDate { get; set; }

    public double Quantity { get; set; }

    public double Price { get; set; }

    public string? Barcode { get; set; }

    public string? MPBarcode { get; set; }

    public string? ImagePath { get; set; }

    public string? Comment { get; set; }

    public string? Image { get; set; }
}
