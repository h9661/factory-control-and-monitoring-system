using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Infrastructure.Data.Configurations;

public class ProductionLineConfiguration : IEntityTypeConfiguration<ProductionLine>
{
    public void Configure(EntityTypeBuilder<ProductionLine> builder)
    {
        builder.ToTable("ProductionLines");

        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pl => pl.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pl => pl.Description)
            .HasMaxLength(1000);

        builder.HasIndex(pl => new { pl.FactoryId, pl.Code })
            .IsUnique();

        builder.HasMany(pl => pl.Equipment)
            .WithOne(e => e.ProductionLine)
            .HasForeignKey(e => e.ProductionLineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
