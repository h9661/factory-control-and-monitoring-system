using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Infrastructure.Data.Configurations;

public class QualityRecordConfiguration : IEntityTypeConfiguration<QualityRecord>
{
    public void Configure(EntityTypeBuilder<QualityRecord> builder)
    {
        builder.ToTable("QualityRecords");

        builder.HasKey(qr => qr.Id);

        builder.Property(qr => qr.DefectDescription)
            .HasMaxLength(1000);

        builder.Property(qr => qr.InspectorId)
            .HasMaxLength(100);

        builder.Property(qr => qr.InspectorName)
            .HasMaxLength(200);

        builder.Property(qr => qr.ImagePath)
            .HasMaxLength(500);

        builder.Property(qr => qr.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(qr => qr.InspectedAt);

        builder.HasIndex(qr => qr.Result);

        builder.HasOne(qr => qr.Equipment)
            .WithMany()
            .HasForeignKey(qr => qr.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
