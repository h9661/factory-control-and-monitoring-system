using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Infrastructure.Data.Configurations;

public class AlarmConfiguration : IEntityTypeConfiguration<Alarm>
{
    public void Configure(EntityTypeBuilder<Alarm> builder)
    {
        builder.ToTable("Alarms");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AlarmCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.AcknowledgedBy)
            .HasMaxLength(100);

        builder.Property(a => a.ResolvedBy)
            .HasMaxLength(100);

        builder.Property(a => a.ResolutionNotes)
            .HasMaxLength(1000);

        builder.HasIndex(a => a.Status);

        builder.HasIndex(a => a.Severity);

        builder.HasIndex(a => a.OccurredAt);
    }
}
