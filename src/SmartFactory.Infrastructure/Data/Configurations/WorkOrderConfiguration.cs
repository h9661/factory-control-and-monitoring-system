using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Infrastructure.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");

        builder.HasKey(wo => wo.Id);

        builder.Property(wo => wo.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(wo => wo.ProductCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wo => wo.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(wo => wo.CustomerName)
            .HasMaxLength(200);

        builder.Property(wo => wo.CustomerOrderRef)
            .HasMaxLength(100);

        builder.HasIndex(wo => wo.OrderNumber)
            .IsUnique();

        builder.HasIndex(wo => wo.Status);

        builder.HasIndex(wo => wo.ScheduledStart);

        builder.HasMany(wo => wo.Steps)
            .WithOne(wos => wos.WorkOrder)
            .HasForeignKey(wos => wos.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(wo => wo.QualityRecords)
            .WithOne(qr => qr.WorkOrder)
            .HasForeignKey(qr => qr.WorkOrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
