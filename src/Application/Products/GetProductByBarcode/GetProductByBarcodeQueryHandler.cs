using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetProductByBarcode;

internal sealed class GetProductByBarcodeQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetProductByBarcodeQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
    {
        var jobItems = await dbContext.JobItems.Where(x =>
            x.RequiredField2 != null &&
            x.RequiredField2 == request.Barcode &&
            (x.RequiredField2.Length == 17 || x.RequiredField2.Length == 21))
        .ToListAsync(cancellationToken);

        var productCode = jobItems.GroupBy(x => x.RequiredField2).Select(g => g.Key).FirstOrDefault();
        if (productCode == null)
            return Result.Failure<ProductResponse>(JobErrors.ProductNotFound);

        var product = jobItems[0];

        productCode = productCode[..^10]; // remove last 10 characters = lot number

        var allJobItemsByProductCode = await dbContext.JobItems.Where(x =>
            x.RequiredField2 != null &&
            (x.RequiredField2.Length == 17 || x.RequiredField2.Length == 21) &&
            x.RequiredField2.StartsWith(productCode))
        .ToListAsync(cancellationToken);

        var jobItemsGroupedByLotNumber = allJobItemsByProductCode
            .GroupBy(x => x.RequiredField2![^10..]) // get last 10 characters = lot number
            .Select(g => new
            {
                LotNumber = g.Key,
                Count = g.Count()
            })
        .ToList();

        var result = new ProductResponse
        {
            ProductDescription = product.ItemDescription ?? string.Empty,
            ProductCode = productCode,
            ProductLotNumbers = [.. jobItemsGroupedByLotNumber.Select(l => l.LotNumber)],
        };

        return result;
    }
}
