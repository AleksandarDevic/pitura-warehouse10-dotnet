using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
{
    public void Configure(EntityTypeBuilder<ProductStock> builder)
    {
        builder.ToTable("StanjeArtikala");

        builder.HasKey(e => new { e.ProductCodeLot, e.WhmCode })
            .HasName("StanjeArtikala_PK");

        builder.Property(e => e.ProductCodeLot)
            .HasMaxLength(36)
            .IsUnicode(false)
            .HasColumnName("SifraLOT");

        builder.Property(e => e.WhmCode)
            .HasColumnName("WhmOznaka")
             .HasMaxLength(18)
            .IsUnicode(false)
            .HasDefaultValue("");

        builder.Property(e => e.Name)
            .HasColumnName("Naziv")
            .HasMaxLength(96)
            .IsUnicode(false);

        builder.Property(e => e.UnitOfMeasure)
            .HasColumnName("JedinicaMere")
            .HasMaxLength(4)
            .IsUnicode(false);

        builder.Property(e => e.Weight)
            .HasColumnName("Tezina");

        builder.Property(e => e.PackageSize)
            .HasColumnName("Pakovanje");

        builder.Property(e => e.ExpirationDate)
            .HasColumnName("RokTrajanja")
            .HasColumnType("datetime");

        builder.Property(e => e.Quantity)
            .HasColumnName("Stanje");

        builder.Property(e => e.Price)
            .HasColumnName("Cena");

        builder.Property(e => e.Barcode)
            .HasColumnName("Barkod")
            .HasMaxLength(52)
            .IsUnicode(false);

        builder.Property(e => e.MPBarcode)
            .HasColumnName("MPBarkod")
            .HasMaxLength(13)
            .IsUnicode(false);

        builder.Property(e => e.ImagePath)
            .HasColumnName("PutanjaDoSlike")
            .HasMaxLength(100)
            .IsUnicode(false);


        builder.Property(e => e.Comment)
            .HasColumnName("Komentar")
            .HasMaxLength(160)
            .IsUnicode(false);

        builder.Property(e => e.Image)
            .HasColumnName("Slika")
            .HasMaxLength(100)
            .IsUnicode(false);
    }
}
