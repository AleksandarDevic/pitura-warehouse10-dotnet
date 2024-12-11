using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("PosloviPocetak");

        builder.HasKey(e => e.Id).HasName("PosloviPocetak_PK");


        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(e => e.Description)
            .HasColumnName("Opis")
            .HasMaxLength(80)
            .IsUnicode(false);

        builder.Property(e => e.AssignedOperatorId)
            .HasColumnName("ZaIDOperatera");

        builder.HasOne(d => d.AssignedOperator)
            .WithMany(p => p.Jobs)
            .HasForeignKey(d => d.AssignedOperatorId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("PosloviPocetak_Operateri_FK");

        builder.Property(e => e.Type)
            .HasColumnName("Tip");

        builder.Property(e => e.CreationDateTime)
            .HasColumnName("VremeKreiranja")
            .HasColumnType("datetime");

        builder.Property(e => e.DueDateTime)
            .HasColumnName("RokZavrsetka")
            .HasColumnType("datetime");

        builder.Property(e => e.TakenOverByOperatorName)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("ImeIPrezimeOperateraKojiJePreuzeo");

        builder.Property(e => e.CompletedByOperatorName)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("ImeIPrezimeOperateraKojiJeZavrsio");

        builder.Property(e => e.CompletionType)
            .HasColumnName("VrstaZavrsetka");

        builder.Property(e => e.LastNote)
            .HasColumnName("PoslednjaNapomena");

        builder.Property(e => e.Field1Length)
            .HasColumnName("Polje1Duzina");

        builder.Property(e => e.IsField1Required)
            .HasColumnName("Polje1Obavezno");

        builder.Property(e => e.Field2Length)
            .HasColumnName("Polje2Duzina");

        builder.Property(e => e.IsField2Required)
            .HasColumnName("Polje2Obavezno");

        builder.Property(e => e.IsField3Required)
            .HasColumnName("Polje3Obavezno");

        builder.Property(e => e.ReadingType)
            .HasColumnName("VrstaOcitavanja");

        builder.Property(e => e.ClosingType)
            .HasColumnName("VrstaZatvaranja");

        builder.Property(e => e.LidderDocumentNumber)
            .HasColumnName("LidderBrojDok");

        builder.Property(e => e.LidderDocumentType)
            .HasColumnName("LidderVrstaDok");

        builder.Property(e => e.InventoryId)
            .HasColumnName("PopisID");

        builder.Property(e => e.IsVerified)
            .HasColumnName("Verifikovano");

        builder.Property(e => e.Client)
            .HasColumnName("Komitent")
            .HasMaxLength(96)
            .IsUnicode(false);
    }
}
