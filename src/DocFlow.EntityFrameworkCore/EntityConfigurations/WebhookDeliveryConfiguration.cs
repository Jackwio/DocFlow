using DocFlow.RoutingQueues;
using DocFlow.EntityFrameworkCore.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace DocFlow.EntityFrameworkCore.EntityConfigurations;

/// <summary>
/// EF Core entity configuration for WebhookDelivery entity.
/// </summary>
public sealed class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTable("WebhookDeliveries");

        builder.ConfigureByConvention();

        builder.Property(d => d.DocumentId)
            .IsRequired();

        builder.Property(d => d.QueueId)
            .IsRequired();

        builder.Property(d => d.AttemptCount)
            .IsRequired();

        builder.Property(d => d.Status)
            .IsRequired();

        builder.Property(d => d.LastError)
            .HasConversion(new ErrorMessageConverter())
            .HasMaxLength(1000);

        builder.Property(d => d.LastAttemptAt);

        builder.Property(d => d.SucceededAt);

        // Indexes
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.DocumentId);
        builder.HasIndex(d => d.QueueId);
    }
}
