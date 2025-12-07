using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Infrastructure.Data.Configurations;

public class SensorDataConfiguration : IEntityTypeConfiguration<SensorData>
{
    public void Configure(EntityTypeBuilder<SensorData> builder)
    {
        builder.ToTable("SensorData");

        builder.HasKey(sd => sd.Id);

        builder.Property(sd => sd.Id)
            .UseIdentityColumn();

        builder.Property(sd => sd.TagName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sd => sd.Unit)
            .HasMaxLength(20);

        // Composite index for time-series queries
        builder.HasIndex(sd => new { sd.EquipmentId, sd.Timestamp })
            .IsDescending(false, true);

        builder.HasIndex(sd => sd.Timestamp);
    }
}
