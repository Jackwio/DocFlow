using DocFlow.Documents;
using DocFlow.EntityFrameworkCore.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace DocFlow.EntityFrameworkCore.EntityConfigurations;

/// <summary>
/// EF Core entity configuration for Document aggregate.
/// </summary>
public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.ConfigureByConvention();

        // Value object conversions
        builder.Property(d => d.FileName)
            .HasConversion(new FileNameConverter())
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.FileSize)
            .HasConversion(new FileSizeConverter())
            .IsRequired();

        builder.Property(d => d.MimeType)
            .HasConversion(new MimeTypeConverter())
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.LastError)
            .HasConversion(new ErrorMessageConverter())
            .HasMaxLength(1000);

        // BlobReference as owned entity
        builder.OwnsOne(d => d.BlobReference, br =>
        {
            br.Property(b => b.ContainerName)
                .HasColumnName("BlobContainerName")
                .HasMaxLength(100)
                .IsRequired();

            br.Property(b => b.BlobName)
                .HasColumnName("BlobName")
                .HasMaxLength(500)
                .IsRequired();
        });

        // Tags as owned collection
        builder.OwnsMany(d => d.Tags, t =>
        {
            t.ToTable("DocumentTags");
            
            t.Property(tag => tag.Name)
                .HasConversion(new TagNameConverter())
                .HasMaxLength(50)
                .IsRequired();

            t.Property(tag => tag.Source)
                .IsRequired();

            // Confidence score for AI tags
            t.OwnsOne(tag => tag.Confidence, cs =>
            {
                cs.Property(s => s.Value)
                    .HasColumnName("ConfidenceScore");
            });

            t.WithOwner().HasForeignKey("DocumentId");
        });

        // AI Suggestion as owned entity
        builder.OwnsOne(d => d.AiSuggestion, ai =>
        {
            ai.Property(a => a.SuggestedQueueId)
                .HasColumnName("AiSuggestedQueueId");

            ai.Property(a => a.Summary)
                .HasColumnName("AiSummary")
                .HasMaxLength(2000);

            ai.Property(a => a.GeneratedAt)
                .HasColumnName("AiGeneratedAt")
                .IsRequired();

            // Overall confidence
            ai.OwnsOne(a => a.Confidence, cs =>
            {
                cs.Property(s => s.Value)
                    .HasColumnName("AiConfidence")
                    .IsRequired();
            });

            // Suggested tags as owned collection
            ai.OwnsMany(a => a.SuggestedTags, st =>
            {
                st.ToTable("DocumentAiSuggestedTags");

                st.Property(s => s.TagName)
                    .HasConversion(new TagNameConverter())
                    .HasMaxLength(50)
                    .IsRequired();

                st.Property(s => s.Reasoning)
                    .HasMaxLength(500);

                st.OwnsOne(s => s.Confidence, cs =>
                {
                    cs.Property(c => c.Value)
                        .HasColumnName("Confidence")
                        .IsRequired();
                });

                st.WithOwner().HasForeignKey("DocumentId");
            });
        });

        // Classification history as owned collection
        builder.OwnsMany(d => d.ClassificationHistory, ch =>
        {
            ch.ToTable("DocumentClassificationHistory");

            ch.Property(c => c.RuleId)
                .IsRequired();

            ch.Property(c => c.TagName)
                .HasConversion(new TagNameConverter())
                .HasMaxLength(50)
                .IsRequired();

            ch.Property(c => c.MatchedCondition)
                .HasMaxLength(500)
                .IsRequired();

            ch.Property(c => c.MatchedAt)
                .IsRequired();

            // ConfidenceScore as value object
            ch.OwnsOne(c => c.ConfidenceScore, cs =>
            {
                cs.Property(s => s.Value)
                    .HasColumnName("ConfidenceScore")
                    .IsRequired();
            });

            ch.WithOwner().HasForeignKey("DocumentId");
        });

        // Indexes
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.TenantId);
        builder.HasIndex(d => d.CreationTime);
    }
}
