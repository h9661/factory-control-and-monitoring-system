using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Infrastructure.Data.Configurations;

public class FactoryConfiguration : IEntityTypeConfiguration<Factory>
{
    public void Configure(EntityTypeBuilder<Factory> builder)
    {
        builder.ToTable("Factories");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Location)
            .HasMaxLength(500);

        builder.Property(f => f.Address)
            .HasMaxLength(500);

        builder.Property(f => f.TimeZone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("UTC");

        builder.Property(f => f.Description)
            .HasMaxLength(1000);

        builder.Property(f => f.ContactEmail)
            .HasMaxLength(200);

        builder.Property(f => f.ContactPhone)
            .HasMaxLength(50);

        builder.Property(f => f.CreatedBy)
            .HasMaxLength(100);

        builder.Property(f => f.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(f => f.DeletedBy)
            .HasMaxLength(100);

        builder.HasIndex(f => f.Code)
            .IsUnique();

        builder.HasMany(f => f.ProductionLines)
            .WithOne(pl => pl.Factory)
            .HasForeignKey(pl => pl.FactoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.WorkOrders)
            .WithOne(wo => wo.Factory)
            .HasForeignKey(wo => wo.FactoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
