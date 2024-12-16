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

        builder.Property(e => e.LoginDateTime)
            .HasColumnName("VremePrijave")
            .HasColumnType("datetime");

        builder.Property(e => e.LogoutDateTime)
            .HasColumnName("VremeOdjave")
            .HasColumnType("datetime");

        builder.HasOne(d => d.Operator)
            .WithMany(p => p.AssignedTerminals)
            .HasForeignKey(d => d.OperatorId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("PrijavaOper_Oper_FK");

        builder.HasOne(d => d.Terminal)
            .WithMany(p => p.AssignedOperators)
            .HasForeignKey(d => d.TerminalId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("PrijavaOper_Term_FK");
    }
}
