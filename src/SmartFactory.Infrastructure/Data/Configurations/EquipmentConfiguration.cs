using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Infrastructure.Data.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.OpcNodeId)
            .HasMaxLength(500);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(50);

        builder.Property(e => e.Manufacturer)
            .HasMaxLength(200);

        builder.Property(e => e.Model)
            .HasMaxLength(200);

        builder.Property(e => e.SerialNumber)
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasIndex(e => new { e.ProductionLineId, e.Code })
            .IsUnique();

        builder.HasIndex(e => e.Status);

        builder.HasMany(e => e.SensorData)
            .WithOne(sd => sd.Equipment)
            .HasForeignKey(sd => sd.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Alarms)
            .WithOne(a => a.Equipment)
            .HasForeignKey(a => a.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.MaintenanceRecords)
            .WithOne(mr => mr.Equipment)
            .HasForeignKey(mr => mr.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
