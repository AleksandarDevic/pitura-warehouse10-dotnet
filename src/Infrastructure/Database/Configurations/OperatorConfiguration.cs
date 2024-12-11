using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
    public void Configure(EntityTypeBuilder<Operator> builder)
    {
        builder.ToTable("Operateri");

        builder.HasKey(e => e.Id)
            .HasName("Operateri_PK");

        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .HasColumnName("Ime")
            .HasMaxLength(15)
            .IsUnicode(false);

        builder.Property(e => e.Password)
            .HasColumnName("Lozinka")
            .HasMaxLength(10)
            .IsUnicode(false);
    }
}
