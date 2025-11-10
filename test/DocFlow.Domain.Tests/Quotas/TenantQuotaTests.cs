using System;
using DocFlow.Quotas;
using Shouldly;
using Xunit;

namespace DocFlow.Domain.Tests.Quotas;

public class TenantQuotaTests
{
    [Fact]
    public void Create_ShouldCreateQuota_WithValidParameters()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var maxDocuments = 1000;
        long maxStorageBytes = 10737418240; // 10GB

        // Act
        var quota = TenantQuota.Create(id, tenantId, maxDocuments, maxStorageBytes);

        // Assert
        quota.Id.ShouldBe(id);
        quota.TenantId.ShouldBe(tenantId);
        quota.MaxDocuments.ShouldBe(maxDocuments);
        quota.MaxStorageBytes.ShouldBe(maxStorageBytes);
        quota.CurrentDocumentCount.ShouldBe(0);
        quota.CurrentStorageBytes.ShouldBe(0);
        quota.IsBlocked.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowException_WhenMaxDocumentsInvalid(int maxDocuments)
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        long maxStorageBytes = 10737418240;

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            TenantQuota.Create(id, tenantId, maxDocuments, maxStorageBytes));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowException_WhenMaxStorageBytesInvalid(long maxStorageBytes)
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var maxDocuments = 1000;

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            TenantQuota.Create(id, tenantId, maxDocuments, maxStorageBytes));
    }

    [Fact]
    public void UpdateUsage_ShouldUpdateCurrentUsage()
    {
        // Arrange
        var quota = TenantQuota.Create(Guid.NewGuid(), Guid.NewGuid(), 1000, 10737418240);
        var newDocumentCount = 500;
        long newStorageBytes = 5368709120; // 5GB

        // Act
        quota.UpdateUsage(newDocumentCount, newStorageBytes);

        // Assert
        quota.CurrentDocumentCount.ShouldBe(newDocumentCount);
        quota.CurrentStorageBytes.ShouldBe(newStorageBytes);
        quota.IsBlocked.ShouldBeFalse();
    }

    [Fact]
    public void UpdateUsage_ShouldBlockUploads_WhenDocumentQuotaExceeded()
    {
        // Arrange
        var quota = TenantQuota.Create(Guid.NewGuid(), Guid.NewGuid(), 1000, 10737418240);

        // Act
        quota.UpdateUsage(1000, 5000000000);

        // Assert
        quota.IsBlocked.ShouldBeTrue();
    }

    [Fact]
    public void UpdateUsage_ShouldBlockUploads_WhenStorageQuotaExceeded()
    {
        // Arrange
        var quota = TenantQuota.Create(Guid.NewGuid(), Guid.NewGuid(), 1000, 10737418240);

        // Act
        quota.UpdateUsage(500, 10737418240);

        // Assert
        quota.IsBlocked.ShouldBeTrue();
    }

    [Fact]
    public void UpdateUsage_ShouldUnblockUploads_WhenQuotaNoLongerExceeded()
    {
        // Arrange
        var quota = TenantQuota.Create(Guid.NewGuid(), Guid.NewGuid(), 1000, 10737418240);
        quota.UpdateUsage(1000, 5000000000); // Exceed quota
        quota.IsBlocked.ShouldBeTrue();

        // Act
        quota.UpdateUsage(500, 5000000000); // Back under quota

        // Assert
        quota.IsBlocked.ShouldBeFalse();
    }

    [Fact]
    public void IsQuotaExceeded_ShouldReturnTrue_WhenDocumentCountReachedMax()
    {
        // Arrange
        var quota = TenantQuota.Create(Guid.NewGuid(), Guid.NewGuid(), 1000, 10737418240);
        quota.UpdateUsage(1000, 5000000000);

        // Act
        var exceeded = quota.IsQuotaExceeded();

        // Assert
        exceeded.ShouldBeTrue();
    }

    [Fact]
    public void IsQuotaExceeded_ShouldReturnTrue_WhenStorageBytesReachedMax()
    {
        // Arrange
        var quota = TenantQuota.Create(Guid.NewGuid(), Guid.NewGuid(), 1000, 10737418240);
        quota.UpdateUsage(500, 10737418240);

        // Act
        var exceeded = quota.IsQuotaExceeded();

        // Assert
        exceeded.ShouldBeTrue();
    }

    [Fact]
    public void IsQuotaExceeded_ShouldReturnFalse_WhenWithinLimits()
    {
        // Arrange
        var quota = TenantQuota.Create(Guid.NewGuid(), Guid.NewGuid(), 1000, 10737418240);
        quota.UpdateUsage(500, 5000000000);

        // Act
        var exceeded = quota.IsQuotaExceeded();

        // Assert
        exceeded.ShouldBeFalse();
    }
}
