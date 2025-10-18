using Application.Abstractions.Messaging;
using Domain.Models;

namespace Application.Products.GetProductByBarcode;

public record GetProductByBarcodeQuery(string Barcode) : IQuery<ProductResponse>;
