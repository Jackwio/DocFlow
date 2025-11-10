using System;
using DocFlow.TenantManagement;
using Shouldly;
using Xunit;

namespace DocFlow.TenantManagement;

public class TenantConfigurationTests
{
    [Fact]
    public void Create_ShouldCreateConfiguration_WithValidParameters()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var retentionDays = RetentionDays.Create(365);
        var maxFileSize = MaxFileSize.FromMegabytes(50);

        // Act
        var config = TenantConfiguration.Create(id, tenantId, retentionDays, maxFileSize);

        // Assert
        config.ShouldNotBeNull();
        config.Id.ShouldBe(id);
        config.TenantId.ShouldBe(tenantId);
        config.RetentionDays.Value.ShouldBe(365);
        config.MaxFileSize.ToMegabytes().ShouldBe(50.0, 0.01);
        config.PrivacyStrictMode.ShouldBeFalse();
        config.WebhookSignatureKey.ShouldBeNull();
        config.TagColors.ShouldBeEmpty();
    }

    [Fact]
    public void UpdateRetentionDays_ShouldUpdateValue()
    {
        // Arrange
        var config = TenantConfiguration.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            RetentionDays.Create(365), 
            MaxFileSize.FromMegabytes(50));
        
        var newRetentionDays = RetentionDays.Create(730);

        // Act
        config.UpdateRetentionDays(newRetentionDays);

        // Assert
        config.RetentionDays.Value.ShouldBe(730);
    }

    [Fact]
    public void SetPrivacyStrictMode_ShouldUpdateValue()
    {
        // Arrange
        var config = TenantConfiguration.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            RetentionDays.Create(365), 
            MaxFileSize.FromMegabytes(50));

        // Act
        config.SetPrivacyStrictMode(true);

        // Assert
        config.PrivacyStrictMode.ShouldBeTrue();
    }

    [Fact]
    public void SetWebhookSignatureKey_ShouldUpdateValue()
    {
        // Arrange
        var config = TenantConfiguration.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            RetentionDays.Create(365), 
            MaxFileSize.FromMegabytes(50));
        
        var key = WebhookSignatureKey.Create("12345678901234567890123456789012");

        // Act
        config.SetWebhookSignatureKey(key);

        // Assert
        config.WebhookSignatureKey.ShouldNotBeNull();
        config.WebhookSignatureKey!.Value.ShouldBe("12345678901234567890123456789012");
    }

    [Fact]
    public void UpdateMaxFileSize_ShouldUpdateValue()
    {
        // Arrange
        var config = TenantConfiguration.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            RetentionDays.Create(365), 
            MaxFileSize.FromMegabytes(50));
        
        var newMaxFileSize = MaxFileSize.FromMegabytes(100);

        // Act
        config.UpdateMaxFileSize(newMaxFileSize);

        // Assert
        config.MaxFileSize.ToMegabytes().ShouldBe(100.0, 0.01);
    }

    [Fact]
    public void ConfigureTagColor_ShouldAddNewTagColor()
    {
        // Arrange
        var config = TenantConfiguration.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            RetentionDays.Create(365), 
            MaxFileSize.FromMegabytes(50));

        // Act
        config.ConfigureTagColor("Invoice", "#FF5733");

        // Assert
        config.TagColors.Count.ShouldBe(1);
        config.TagColors.ShouldContain(tc => tc.TagName == "Invoice" && tc.ColorHex == "#FF5733");
    }

    [Fact]
    public void ConfigureTagColor_ShouldReplaceExistingTagColor()
    {
        // Arrange
        var config = TenantConfiguration.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            RetentionDays.Create(365), 
            MaxFileSize.FromMegabytes(50));
        config.ConfigureTagColor("Invoice", "#FF5733");

        // Act
        config.ConfigureTagColor("Invoice", "#33FF57");

        // Assert
        config.TagColors.Count.ShouldBe(1);
        config.TagColors.ShouldContain(tc => tc.TagName == "Invoice" && tc.ColorHex == "#33FF57");
    }

    [Fact]
    public void RemoveTagColor_ShouldRemoveTagColor()
    {
        // Arrange
        var config = TenantConfiguration.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            RetentionDays.Create(365), 
            MaxFileSize.FromMegabytes(50));
        config.ConfigureTagColor("Invoice", "#FF5733");

        // Act
        config.RemoveTagColor("Invoice");

        // Assert
        config.TagColors.ShouldBeEmpty();
    }
}
