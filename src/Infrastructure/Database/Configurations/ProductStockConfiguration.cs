using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
{
    public void Configure(EntityTypeBuilder<ProductStock> builder)
    {
        builder.ToTable("StanjeArtikala");

        builder.HasKey(e => e.LotCode)
            .HasName("StanjeArtikala_PK");

        builder.Property(e => e.LotCode)
            .HasMaxLength(21)
            .IsUnicode(false)
            .HasColumnName("SifraLOT");

        builder.Property(e => e.WhmCode)
            .HasColumnName("WhmOznaka")
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(e => e.Name)
            .HasColumnName("Naziv")
            .HasMaxLength(150)
            .IsUnicode(false);

        builder.Property(e => e.UnitOfMeasure)
            .HasColumnName("JedinicaMere")
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(e => e.Weight)
            .HasColumnName("Tezina");

        builder.Property(e => e.PackageSize)
            .HasColumnName("Pakovanje");

        builder.Property(e => e.ExpirationDate)
            .HasColumnName("RokTrajanja")
            .HasColumnType("datetime");

        builder.Property(e => e.Quantity)
            .HasColumnName("Stanje")
            .HasColumnType("decimal(38, 0)");

        builder.Property(e => e.Price)
            .HasColumnName("Cena")
            .HasColumnType("decimal(38, 0)");

        builder.Property(e => e.Barcode)
            .HasColumnName("Barkod")
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(e => e.MPBarcode)
            .HasColumnName("MPBarkod")
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(e => e.ImagePath)
            .HasColumnName("PutanjaDoSlike")
            .HasMaxLength(100)
            .IsUnicode(false);


        builder.Property(e => e.Comment)
            .HasColumnName("Komentar")
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(e => e.Image)
            .HasColumnName("Slika")
            .HasMaxLength(100)
            .IsUnicode(false);
    }
}
