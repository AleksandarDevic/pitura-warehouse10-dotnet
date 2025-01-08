namespace Application.JobItems.InsertJobItemToInventory;
public record InsertJobItemToInventoryRequest
{
    public string ReadedField1 { get; set; } = null!;
    public string ReadedField2 { get; set; } = null!;
    public double ReadedField3 { get; init; }
}
