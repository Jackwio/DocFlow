using DocFlow.RoutingQueues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace DocFlow.EntityFrameworkCore.EntityConfigurations;

/// <summary>
/// EF Core entity configuration for RoutingQueue aggregate.
/// </summary>
public sealed class RoutingQueueConfiguration : IEntityTypeConfiguration<RoutingQueue>
{
    public void Configure(EntityTypeBuilder<RoutingQueue> builder)
    {
        builder.ToTable("RoutingQueues");

        builder.ConfigureByConvention();

        builder.Property(q => q.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(q => q.Description)
            .HasMaxLength(1000);

        builder.Property(q => q.Type)
            .IsRequired();

        builder.Property(q => q.IsActive)
            .IsRequired();

        // FolderPath as owned value object
        builder.OwnsOne(q => q.FolderPath, f =>
        {
            f.Property(fp => fp.Value)
                .HasColumnName("FolderPath")
                .HasMaxLength(500);
        });

        // WebhookConfiguration as owned value object
        builder.OwnsOne(q => q.WebhookConfiguration, w =>
        {
            w.Property(wc => wc.Url)
                .HasColumnName("WebhookUrl")
                .HasMaxLength(500);

            w.Property(wc => wc.MaxRetryAttempts)
                .HasColumnName("WebhookMaxRetryAttempts");

            w.Property(wc => wc.RetryDelaySeconds)
                .HasColumnName("WebhookRetryDelaySeconds");

            // Store headers as JSON
            w.Property(wc => wc.Headers)
                .HasColumnName("WebhookHeaders")
                .HasColumnType("jsonb");
        });

        // Indexes
        builder.HasIndex(q => q.Type);
        builder.HasIndex(q => q.IsActive);
        builder.HasIndex(q => q.TenantId);
    }
}
