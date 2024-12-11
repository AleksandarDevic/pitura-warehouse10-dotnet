using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class JobInProgressConfiguration : IEntityTypeConfiguration<JobInProgress>
{
    public void Configure(EntityTypeBuilder<JobInProgress> builder)
    {
        builder.ToTable("PosloviUToku");

        builder.HasKey(e => e.Id)
            .HasName("PosloviUToku_PK");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("ID");

        builder.Property(e => e.JobId)
            .HasColumnName("IDPosla");

        builder.HasOne(d => d.Job)
            .WithMany(p => p.JobsInProgress)
            .HasForeignKey(d => d.JobId)
            .HasConstraintName("PosloviUToku_PosloviPocetak_FK");

        builder.Property(e => e.OperatorTerminalId)
            .HasColumnName("IDPrijaveOperatera");

        builder.HasOne(d => d.OperatorTerminal)
            .WithMany(p => p.JobsInProgess)
            .HasForeignKey(d => d.OperatorTerminalId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("PosloviUToku_PrijavaOperatera_FK");

        builder.Property(e => e.StartDateTime)
            .HasColumnName("VremePreuzimanja")
            .HasColumnType("datetime");

        builder.Property(e => e.EndDateTime)
            .HasColumnName("VremeZavrsetka")
            .HasColumnType("datetime");

        builder.Property(e => e.CompletionType)
            .HasColumnName("VrstaZavrsetka");

        builder.Property(e => e.Note)
            .HasColumnName("Napomena")
            .HasMaxLength(240)
            .IsUnicode(false);
    }
}
