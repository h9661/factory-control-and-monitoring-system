using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Infrastructure.Data.Configurations;

public class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("MaintenanceRecords");

        builder.HasKey(mr => mr.Id);

        builder.Property(mr => mr.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(mr => mr.Description)
            .HasMaxLength(1000);

        builder.Property(mr => mr.TechnicianId)
            .HasMaxLength(100);

        builder.Property(mr => mr.TechnicianName)
            .HasMaxLength(200);

        builder.Property(mr => mr.PartsUsed)
            .HasMaxLength(1000);

        builder.Property(mr => mr.EstimatedCost)
            .HasPrecision(18, 2);

        builder.Property(mr => mr.ActualCost)
            .HasPrecision(18, 2);

        builder.HasIndex(mr => mr.ScheduledDate);

        builder.HasIndex(mr => mr.Status);

        builder.HasIndex(mr => mr.Type);
    }
}
