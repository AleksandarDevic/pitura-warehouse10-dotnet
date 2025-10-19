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
        if (request.Barcode.Length != 17 && request.Barcode.Length != 21)
            return Result.Failure<ProductResponse>(JobErrors.ProductInvalidBarcodeFormat);

        var productCode = request.Barcode[..^10]; // remove last 10 characters = lot number

        var jobItems = await dbContext.JobItems.Where(x =>
            x.RequiredField2 != null &&
            x.RequiredField2.StartsWith(productCode) &&
            (x.RequiredField2.Length == 17 || x.RequiredField2.Length == 21))
        .ToListAsync(cancellationToken);

        if (jobItems.Count == 0)
        {
            var productStock = await dbContext.ProductStocks
                .Where(x => x.ProductCodeLot.StartsWith(productCode))
            .FirstOrDefaultAsync(cancellationToken);

            if (productStock is null)
                return Result.Failure<ProductResponse>(JobErrors.ProductNotFound);

            return new ProductResponse
            {
                ProductDescription = productStock.Name.Trim() ?? string.Empty,
                WhmCode = productStock.WhmCode ?? string.Empty,
                ProductCode = productCode,
                ProductLotNumber = productStock.ProductCodeLot[^10..],
                ProductLotNumbers = [],
            };
        }

        var product = jobItems[0];

        productCode = productCode[..^10]; // remove last 10 characters = lot number
        var productLotNumber = product.RequiredField2![^10..];

        var jobItemsGroupedByLotNumber = jobItems
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
            WhmCode = product.RequiredField1 ?? string.Empty,
            ProductCode = productCode,
            ProductLotNumber = productLotNumber,
            ProductLotNumbers = [.. jobItemsGroupedByLotNumber.Select(l => l.LotNumber)],
        };

        return result;
    }
}
