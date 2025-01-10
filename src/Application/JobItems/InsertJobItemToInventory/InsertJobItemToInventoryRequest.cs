namespace Application.JobItems.InsertJobItemToInventory;
public record InsertJobItemToInventoryRequest
{
    public string RequiredFieldRead1 { get; set; } = null!;
    public string RequiredFieldRead2 { get; set; } = null!;
    public double RequiredFieldRead3 { get; init; }
}
