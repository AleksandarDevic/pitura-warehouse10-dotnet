using Application.Products.GetProductByBarcode;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints;

public class Product : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {

        app.MapGet("product/barcode/{requiredFieldRead2}/scan", async (
            ISender sender,
            [FromRoute] string requiredFieldRead2,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductByBarcodeQuery(requiredFieldRead2);
            var result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Product);
    }
}