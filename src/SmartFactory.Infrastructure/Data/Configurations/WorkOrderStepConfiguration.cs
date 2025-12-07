using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Infrastructure.Data.Configurations;

public class WorkOrderStepConfiguration : IEntityTypeConfiguration<WorkOrderStep>
{
    public void Configure(EntityTypeBuilder<WorkOrderStep> builder)
    {
        builder.ToTable("WorkOrderSteps");

        builder.HasKey(wos => wos.Id);

        builder.Property(wos => wos.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(wos => new { wos.WorkOrderId, wos.Sequence });

        builder.HasOne(wos => wos.Equipment)
            .WithMany()
            .HasForeignKey(wos => wos.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
