using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class TerminalConfiguration : IEntityTypeConfiguration<Terminal>
{
    public void Configure(EntityTypeBuilder<Terminal> builder)
    {
        builder.ToTable("Terminali");

        builder.HasKey(e => e.Id)
            .HasName("Terminali_PK");

        builder.Property(e => e.Id)
            .HasColumnName("ID");

        builder.Property(e => e.Name)
            .HasColumnName("Naziv")
            .HasMaxLength(15)
            .IsUnicode(false);

        builder.HasIndex(e => e.Name, "UQ__Terminal__603E8146582D883C")
            .IsUnique();

        builder.Property(e => e.IsActive)
           .HasColumnName("Aktivan")
           .HasDefaultValue(true);
    }
}
