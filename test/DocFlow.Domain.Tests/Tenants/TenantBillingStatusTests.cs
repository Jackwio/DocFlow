using System;
using DocFlow.Tenants;
using Shouldly;
using Xunit;

namespace DocFlow.Domain.Tests.Tenants;

public class TenantBillingStatusTests
{
    [Fact]
    public void Create_ShouldCreateBillingStatus_WithActiveStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var gracePeriodDays = 7;

        // Act
        var billingStatus = TenantBillingStatus.Create(id, tenantId, gracePeriodDays);

        // Assert
        billingStatus.Id.ShouldBe(id);
        billingStatus.TenantId.ShouldBe(tenantId);
        billingStatus.Status.ShouldBe(BillingStatus.Active);
        billingStatus.GracePeriodDays.ShouldBe(gracePeriodDays);
        billingStatus.PaymentFailureDate.ShouldBeNull();
        billingStatus.GracePeriodEndDate.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenGracePeriodDaysNegative()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            TenantBillingStatus.Create(id, tenantId, -1));
    }

    [Fact]
    public void MarkPaymentFailed_ShouldSetPaymentFailedStatus_WhenActive()
    {
        // Arrange
        var billingStatus = TenantBillingStatus.Create(Guid.NewGuid(), Guid.NewGuid(), 7);

        // Act
        billingStatus.MarkPaymentFailed();

        // Assert
        billingStatus.Status.ShouldBe(BillingStatus.PaymentFailed);
        billingStatus.PaymentFailureDate.ShouldNotBeNull();
        billingStatus.GracePeriodEndDate.ShouldNotBeNull();
    }

    [Fact]
    public void MarkPaymentFailed_ShouldSetGracePeriodEndDate_BasedOnGracePeriodDays()
    {
        // Arrange
        var gracePeriodDays = 7;
        var billingStatus = TenantBillingStatus.Create(Guid.NewGuid(), Guid.NewGuid(), gracePeriodDays);
        var beforeCall = DateTime.UtcNow;

        // Act
        billingStatus.MarkPaymentFailed();

        // Assert
        var afterCall = DateTime.UtcNow.AddDays(gracePeriodDays);
        billingStatus.GracePeriodEndDate.ShouldNotBeNull();
        billingStatus.GracePeriodEndDate.Value.ShouldBeInRange(
            beforeCall.AddDays(gracePeriodDays).AddSeconds(-5),
            afterCall.AddSeconds(5));
    }

    [Fact]
    public void MarkAsReadOnly_ShouldSetReadOnlyStatus_WhenPaymentFailed()
    {
        // Arrange
        var billingStatus = TenantBillingStatus.Create(Guid.NewGuid(), Guid.NewGuid(), 7);
        billingStatus.MarkPaymentFailed();

        // Act
        billingStatus.MarkAsReadOnly();

        // Assert
        billingStatus.Status.ShouldBe(BillingStatus.ReadOnly);
    }

    [Fact]
    public void MarkAsReadOnly_ShouldNotChangeStatus_WhenNotPaymentFailed()
    {
        // Arrange
        var billingStatus = TenantBillingStatus.Create(Guid.NewGuid(), Guid.NewGuid(), 7);

        // Act
        billingStatus.MarkAsReadOnly();

        // Assert
        billingStatus.Status.ShouldBe(BillingStatus.Active);
    }

    [Fact]
    public void RestoreToActive_ShouldResetBillingStatus()
    {
        // Arrange
        var billingStatus = TenantBillingStatus.Create(Guid.NewGuid(), Guid.NewGuid(), 7);
        billingStatus.MarkPaymentFailed();
        billingStatus.MarkAsReadOnly();

        // Act
        billingStatus.RestoreToActive();

        // Assert
        billingStatus.Status.ShouldBe(BillingStatus.Active);
        billingStatus.PaymentFailureDate.ShouldBeNull();
        billingStatus.GracePeriodEndDate.ShouldBeNull();
    }

    [Fact]
    public void IsGracePeriodExpired_ShouldReturnFalse_WhenStatusNotPaymentFailed()
    {
        // Arrange
        var billingStatus = TenantBillingStatus.Create(Guid.NewGuid(), Guid.NewGuid(), 7);

        // Act
        var isExpired = billingStatus.IsGracePeriodExpired();

        // Assert
        isExpired.ShouldBeFalse();
    }

    [Fact]
    public void IsGracePeriodExpired_ShouldReturnFalse_WhenGracePeriodNotYetExpired()
    {
        // Arrange
        var billingStatus = TenantBillingStatus.Create(Guid.NewGuid(), Guid.NewGuid(), 7);
        billingStatus.MarkPaymentFailed();

        // Act
        var isExpired = billingStatus.IsGracePeriodExpired();

        // Assert
        isExpired.ShouldBeFalse();
    }
}
