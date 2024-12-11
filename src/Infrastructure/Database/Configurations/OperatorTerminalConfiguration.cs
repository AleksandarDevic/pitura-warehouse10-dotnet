using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class OperatorTerminalConfiguration : IEntityTypeConfiguration<OperatorTerminal>
{
    public void Configure(EntityTypeBuilder<OperatorTerminal> builder)
    {
        builder.ToTable("PrijavaOperatera");

        builder.HasKey(e => e.Id)
            .HasName("PrijavaOperatera_PK");

        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(e => e.OperatorId)
            .HasColumnName("IDOperatera");

        builder.Property(e => e.TerminalId)
            .HasColumnName("IDTerminala");

        builder.Property(e => e.LoginTime)
            .HasColumnName("VremePrijave")
            .HasColumnType("datetime");

        builder.Property(e => e.LogoutTime)
            .HasColumnName("VremeOdjave")
            .HasColumnType("datetime");

        builder.HasOne(d => d.Operator)
            .WithMany(p => p.AssignedTerminals)
            .HasForeignKey(d => d.OperatorId)
            .HasConstraintName("PrijavaOperatera_Operateri_FK_1");

        builder.HasOne(d => d.Terminal)
            .WithMany(p => p.AssignedOperators)
            .HasForeignKey(d => d.TerminalId)
            .HasConstraintName("PrijavaOperatera_Terminali_FK");
    }
}
