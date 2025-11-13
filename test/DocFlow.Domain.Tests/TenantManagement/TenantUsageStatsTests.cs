using System;
using DocFlow.TenantManagement;
using Shouldly;
using Xunit;

namespace DocFlow.TenantManagement;

public class TenantUsageStatsTests
{
    [Fact]
    public void Create_ShouldCreateStats_WithZeroValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var stats = TenantUsageStats.Create(id, tenantId);

        // Assert
        stats.ShouldNotBeNull();
        stats.Id.ShouldBe(id);
        stats.TenantId.ShouldBe(tenantId);
        stats.DocumentCount.Value.ShouldBe(0);
        stats.StorageUsage.Bytes.ShouldBe(0);
        stats.RuleCount.Value.ShouldBe(0);
    }

    [Fact]
    public void RecordDocumentUpload_ShouldIncrementCountAndStorage()
    {
        // Arrange
        var stats = TenantUsageStats.Create(Guid.NewGuid(), Guid.NewGuid());
        var fileSize = 1024 * 1024; // 1MB

        // Act
        stats.RecordDocumentUpload(fileSize);

        // Assert
        stats.DocumentCount.Value.ShouldBe(1);
        stats.StorageUsage.Bytes.ShouldBe(fileSize);
    }

    [Fact]
    public void RecordDocumentDeletion_ShouldDecrementCountAndStorage()
    {
        // Arrange
        var stats = TenantUsageStats.Create(Guid.NewGuid(), Guid.NewGuid());
        var fileSize = 1024 * 1024;
        stats.RecordDocumentUpload(fileSize);

        // Act
        stats.RecordDocumentDeletion(fileSize);

        // Assert
        stats.DocumentCount.Value.ShouldBe(0);
        stats.StorageUsage.Bytes.ShouldBe(0);
    }

    [Fact]
    public void RecordRuleCreated_ShouldIncrementRuleCount()
    {
        // Arrange
        var stats = TenantUsageStats.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        stats.RecordRuleCreated();

        // Assert
        stats.RuleCount.Value.ShouldBe(1);
    }

    [Fact]
    public void RecordRuleDeleted_ShouldDecrementRuleCount()
    {
        // Arrange
        var stats = TenantUsageStats.Create(Guid.NewGuid(), Guid.NewGuid());
        stats.RecordRuleCreated();

        // Act
        stats.RecordRuleDeleted();

        // Assert
        stats.RuleCount.Value.ShouldBe(0);
    }

    [Fact]
    public void Refresh_ShouldUpdateAllValues()
    {
        // Arrange
        var stats = TenantUsageStats.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        stats.Refresh(10, 10485760, 5); // 10 docs, 10MB, 5 rules

        // Assert
        stats.DocumentCount.Value.ShouldBe(10);
        stats.StorageUsage.Bytes.ShouldBe(10485760);
        stats.RuleCount.Value.ShouldBe(5);
    }

    [Fact]
    public void RecordDocumentUpload_ShouldThrowException_WhenFileSizeNegative()
    {
        // Arrange
        var stats = TenantUsageStats.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        Should.Throw<ArgumentException>(() => stats.RecordDocumentUpload(-1));
    }
}

public class DocumentCountTests
{
    [Fact]
    public void Create_ShouldCreateDocumentCount_WithValidValue()
    {
        // Act
        var count = DocumentCount.Create(10);

        // Assert
        count.Value.ShouldBe(10);
    }

    [Fact]
    public void Increment_ShouldIncreaseValue()
    {
        // Arrange
        var count = DocumentCount.Create(5);

        // Act
        var newCount = count.Increment();

        // Assert
        newCount.Value.ShouldBe(6);
    }

    [Fact]
    public void Decrement_ShouldDecreaseValue()
    {
        // Arrange
        var count = DocumentCount.Create(5);

        // Act
        var newCount = count.Decrement();

        // Assert
        newCount.Value.ShouldBe(4);
    }

    [Fact]
    public void Decrement_ShouldThrowException_WhenValueIsZero()
    {
        // Arrange
        var count = DocumentCount.Create(0);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => count.Decrement());
    }

    [Fact]
    public void Create_ShouldThrowException_WhenValueIsNegative()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => DocumentCount.Create(-1));
    }
}

public class StorageUsageTests
{
    [Fact]
    public void Create_ShouldCreateStorageUsage_WithValidBytes()
    {
        // Act
        var storage = StorageUsage.Create(1024 * 1024);

        // Assert
        storage.Bytes.ShouldBe(1024 * 1024);
        storage.ToMegabytes().ShouldBe(1.0, 0.01);
    }

    [Fact]
    public void Add_ShouldIncreaseBytes()
    {
        // Arrange
        var storage = StorageUsage.Create(1024);

        // Act
        var newStorage = storage.Add(1024);

        // Assert
        newStorage.Bytes.ShouldBe(2048);
    }

    [Fact]
    public void Subtract_ShouldDecreaseBytes()
    {
        // Arrange
        var storage = StorageUsage.Create(2048);

        // Act
        var newStorage = storage.Subtract(1024);

        // Assert
        newStorage.Bytes.ShouldBe(1024);
    }

    [Fact]
    public void Subtract_ShouldNotGoBelowZero()
    {
        // Arrange
        var storage = StorageUsage.Create(1024);

        // Act
        var newStorage = storage.Subtract(2048);

        // Assert
        newStorage.Bytes.ShouldBe(0);
    }

    [Fact]
    public void ToGigabytes_ShouldConvertCorrectly()
    {
        // Arrange
        var storage = StorageUsage.Create(1024L * 1024 * 1024); // 1GB

        // Act
        var gigabytes = storage.ToGigabytes();

        // Assert
        gigabytes.ShouldBe(1.0, 0.01);
    }
}

public class RuleCountTests
{
    [Fact]
    public void Create_ShouldCreateRuleCount_WithValidValue()
    {
        // Act
        var count = RuleCount.Create(5);

        // Assert
        count.Value.ShouldBe(5);
    }

    [Fact]
    public void Increment_ShouldIncreaseValue()
    {
        // Arrange
        var count = RuleCount.Create(3);

        // Act
        var newCount = count.Increment();

        // Assert
        newCount.Value.ShouldBe(4);
    }

    [Fact]
    public void Decrement_ShouldDecreaseValue()
    {
        // Arrange
        var count = RuleCount.Create(3);

        // Act
        var newCount = count.Decrement();

        // Assert
        newCount.Value.ShouldBe(2);
    }

    [Fact]
    public void Decrement_ShouldThrowException_WhenValueIsZero()
    {
        // Arrange
        var count = RuleCount.Create(0);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => count.Decrement());
    }

    [Fact]
    public void Create_ShouldThrowException_WhenValueIsNegative()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => RuleCount.Create(-1));
    }
}
