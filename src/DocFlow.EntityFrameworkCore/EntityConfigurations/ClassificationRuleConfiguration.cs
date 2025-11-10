using DocFlow.ClassificationRules;
using DocFlow.EntityFrameworkCore.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace DocFlow.EntityFrameworkCore.EntityConfigurations;

/// <summary>
/// EF Core entity configuration for ClassificationRule aggregate.
/// </summary>
public sealed class ClassificationRuleConfiguration : IEntityTypeConfiguration<ClassificationRule>
{
    public void Configure(EntityTypeBuilder<ClassificationRule> builder)
    {
        builder.ToTable("ClassificationRules");

        builder.ConfigureByConvention();

        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.IsActive)
            .IsRequired();

        // RulePriority as value object
        builder.OwnsOne(r => r.Priority, p =>
        {
            p.Property(pr => pr.Value)
                .HasColumnName("Priority")
                .IsRequired();
        });

        // ApplyTags as owned collection (TagName is a value object with a string Value)
        builder.OwnsMany(r => r.ApplyTags, t =>
        {
            t.ToTable("ClassificationRuleTags");

            // The owned type is TagName whose `Value` property is already a string,
            // so configure the `Value` column directly (no converter needed here).
            t.Property(tagName => tagName.Value)
                .HasColumnName("TagName")
                .HasMaxLength(50)
                .IsRequired();

            t.WithOwner().HasForeignKey("ClassificationRuleId");
        });

        // Conditions as owned collection
        builder.OwnsMany(r => r.Conditions, c =>
        {
            c.ToTable("ClassificationRuleConditions");

            c.Property(cond => cond.Type)
                .IsRequired();

            c.Property(cond => cond.Pattern)
                .HasMaxLength(500)
                .IsRequired();

            c.Property(cond => cond.MatchValue)
                .HasMaxLength(500);

            c.WithOwner().HasForeignKey("ClassificationRuleId");
        });

        // Indexes
        builder.HasIndex(r => r.IsActive);
        builder.HasIndex(r => r.TenantId);
    }
}
