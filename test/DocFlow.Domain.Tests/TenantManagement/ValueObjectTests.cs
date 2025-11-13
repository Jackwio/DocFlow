using System;
using DocFlow.TenantManagement;
using Shouldly;
using Xunit;

namespace DocFlow.TenantManagement;

public class RetentionDaysTests
{
    [Theory]
    [InlineData(30)]
    [InlineData(365)]
    [InlineData(730)]
    [InlineData(3650)]
    public void Create_ShouldCreateRetentionDays_WithValidDays(int days)
    {
        // Act
        var retentionDays = RetentionDays.Create(days);

        // Assert
        retentionDays.ShouldNotBeNull();
        retentionDays.Value.ShouldBe(days);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDaysTooLow()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => RetentionDays.Create(29));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDaysTooHigh()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => RetentionDays.Create(3651));
    }
}

public class MaxFileSizeTests
{
    [Fact]
    public void Create_ShouldCreateMaxFileSize_WithValidBytes()
    {
        // Act
        var maxFileSize = MaxFileSize.Create(1024 * 1024); // 1MB

        // Assert
        maxFileSize.ShouldNotBeNull();
        maxFileSize.Bytes.ShouldBe(1024 * 1024);
        maxFileSize.ToMegabytes().ShouldBe(1.0, 0.01);
    }

    [Fact]
    public void FromMegabytes_ShouldCreateMaxFileSize()
    {
        // Act
        var maxFileSize = MaxFileSize.FromMegabytes(50);

        // Assert
        maxFileSize.ToMegabytes().ShouldBe(50.0, 0.01);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenBytesTooLow()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => MaxFileSize.Create(1023));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenBytesTooHigh()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => MaxFileSize.Create(104857601)); // >100MB
    }

    [Fact]
    public void FromMegabytes_ShouldThrowException_WhenMegabytesTooLow()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => MaxFileSize.FromMegabytes(0));
    }
}

public class WebhookSignatureKeyTests
{
    [Fact]
    public void Create_ShouldCreateKey_WithValidValue()
    {
        // Arrange
        var value = "12345678901234567890123456789012";

        // Act
        var key = WebhookSignatureKey.Create(value);

        // Assert
        key.ShouldNotBeNull();
        key.Value.ShouldBe(value);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenValueTooShort()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => 
            WebhookSignatureKey.Create("short"));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenValueTooLong()
    {
        // Arrange
        var longValue = new string('a', 257);

        // Act & Assert
        Should.Throw<ArgumentException>(() => 
            WebhookSignatureKey.Create(longValue));
    }

    [Fact]
    public void ToString_ShouldMaskKey()
    {
        // Arrange
        var key = WebhookSignatureKey.Create("12345678901234567890123456789012");

        // Act
        var maskedValue = key.ToString();

        // Assert
        maskedValue.ShouldStartWith("***");
        maskedValue.ShouldEndWith("9012");
    }
}

public class TagColorConfigurationTests
{
    [Fact]
    public void Create_ShouldCreateTagColor_WithValidParameters()
    {
        // Act
        var tagColor = TagColorConfiguration.Create("Invoice", "#FF5733");

        // Assert
        tagColor.ShouldNotBeNull();
        tagColor.TagName.ShouldBe("Invoice");
        tagColor.ColorHex.ShouldBe("#FF5733");
    }

    [Fact]
    public void Create_ShouldNormalizeHexColor()
    {
        // Act
        var tagColor = TagColorConfiguration.Create("Invoice", "ff5733");

        // Assert
        tagColor.ColorHex.ShouldBe("#FF5733");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenTagNameIsEmpty()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => 
            TagColorConfiguration.Create("", "#FF5733"));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenColorHexIsInvalid()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => 
            TagColorConfiguration.Create("Invoice", "invalid"));
    }

    [Theory]
    [InlineData("#FF5733")]
    [InlineData("#ff5733")]
    [InlineData("FF5733")]
    [InlineData("ff5733")]
    public void Create_ShouldAcceptValidHexColors(string colorHex)
    {
        // Act
        var tagColor = TagColorConfiguration.Create("Invoice", colorHex);

        // Assert
        tagColor.ShouldNotBeNull();
    }
}
