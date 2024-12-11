using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class JobItemConfiguration : IEntityTypeConfiguration<JobItem>
{
    public void Configure(EntityTypeBuilder<JobItem> builder)
    {
        builder.ToTable("StavkePosla");

        builder.HasKey(e => e.Id)
            .HasName("StavkePosla_PK");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("ID");

        builder.Property(e => e.JobId)
            .HasColumnName("IDPosaoPocetak");

        builder.HasOne(d => d.Job)
            .WithMany(p => p.JobItems)
            .HasForeignKey(d => d.JobId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("StavkePosla_PosloviPocetak_FK");

        builder.Property(e => e.ItemDescription)
            .HasColumnName("OpisStavke")
            .HasMaxLength(150)
            .IsUnicode(false);

        builder.Property(e => e.RequiredField1)
            .HasColumnName("ReqPolje1")
            .HasMaxLength(18)
            .IsUnicode(false);

        builder.Property(e => e.RequiredField2)
            .HasColumnName("ReqPolje2")
            .HasMaxLength(36)
            .IsUnicode(false);

        builder.Property(e => e.RequiredField3)
            .HasColumnName("ReqPolje3");

        builder.Property(e => e.JobInProgressId)
            .HasColumnName("IDPosaoUToku");

        builder.HasOne(d => d.JobInProgress)
            .WithMany(p => p.JobItems)
            .HasForeignKey(d => d.JobInProgressId)
            .HasConstraintName("StavkePosla_PosloviUToku_FK");

        builder.Property(e => e.ReadedField3)
            .HasColumnName("ReadedPolje3");

        builder.Property(e => e.ItemStatus)
            .HasColumnName("StatusStavke");
    }
}
