using SharedKernel;

namespace Application.JobItems.GetJobItems;

public record GetJobItemsRequest : BasePagedRequest
{
    public long? AnchorItemId { get; set; }
}
