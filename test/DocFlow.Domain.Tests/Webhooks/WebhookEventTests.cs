using System;
using DocFlow.Webhooks;
using Shouldly;
using Xunit;

namespace DocFlow.Domain.Tests.Webhooks;

public class WebhookEventTests
{
    [Fact]
    public void Create_ShouldCreateWebhookEvent_WithPendingStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var eventType = "document.classified";
        var payload = "{\"documentId\":\"123\"}";
        var targetUrl = "https://example.com/webhook";

        // Act
        var webhook = WebhookEvent.Create(id, tenantId, eventType, payload, targetUrl);

        // Assert
        webhook.Id.ShouldBe(id);
        webhook.TenantId.ShouldBe(tenantId);
        webhook.EventType.ShouldBe(eventType);
        webhook.Payload.ShouldBe(payload);
        webhook.TargetUrl.ShouldBe(targetUrl);
        webhook.Status.ShouldBe(WebhookStatus.Pending);
        webhook.RetryCount.ShouldBe(0);
        webhook.HmacSignature.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldThrowException_WhenEventTypeInvalid(string eventType)
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), eventType, "{}", "https://example.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldThrowException_WhenPayloadInvalid(string payload)
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "test.event", payload, "https://example.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldThrowException_WhenTargetUrlInvalid(string targetUrl)
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "test.event", "{}", targetUrl));
    }

    [Fact]
    public void SetHmacSignature_ShouldSetSignature()
    {
        // Arrange
        var webhook = WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "test.event", "{}", "https://example.com");
        var signature = "abcdef123456";

        // Act
        webhook.SetHmacSignature(signature);

        // Assert
        webhook.HmacSignature.ShouldBe(signature);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void SetHmacSignature_ShouldThrowException_WhenSignatureInvalid(string signature)
    {
        // Arrange
        var webhook = WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "test.event", "{}", "https://example.com");

        // Act & Assert
        Should.Throw<ArgumentException>(() => webhook.SetHmacSignature(signature));
    }

    [Fact]
    public void MarkAsSending_ShouldChangeStatus_WhenPending()
    {
        // Arrange
        var webhook = WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "test.event", "{}", "https://example.com");

        // Act
        webhook.MarkAsSending();

        // Assert
        webhook.Status.ShouldBe(WebhookStatus.Sending);
    }

    [Fact]
    public void MarkAsSending_ShouldThrowException_WhenNotPendingOrFailed()
    {
        // Arrange
        var webhook = WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "test.event", "{}", "https://example.com");
        webhook.MarkAsSending();
        webhook.MarkAsSent();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => webhook.MarkAsSending());
    }

    [Fact]
    public void MarkAsSent_ShouldSetSentStatusAndTime()
    {
        // Arrange
        var webhook = WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "test.event", "{}", "https://example.com");
        var beforeSent = DateTime.UtcNow;

        // Act
        webhook.MarkAsSending();
        webhook.MarkAsSent();

        // Assert
        webhook.Status.ShouldBe(WebhookStatus.Sent);
        webhook.SentTime.ShouldNotBeNull();
        webhook.SentTime.Value.ShouldBeGreaterThanOrEqualTo(beforeSent);
    }

    [Fact]
    public void MarkAsFailed_ShouldIncrementRetryCount()
    {
        // Arrange
        var webhook = WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "test.event", "{}", "https://example.com");

        // Act
        webhook.MarkAsFailed();

        // Assert
        webhook.Status.ShouldBe(WebhookStatus.Failed);
        webhook.RetryCount.ShouldBe(1);
    }

    [Fact]
    public void CanRetry_ShouldReturnTrue_WhenRetryCountBelowMax()
    {
        // Arrange
        var webhook = WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "test.event", "{}", "https://example.com");
        webhook.MarkAsFailed();

        // Act
        var canRetry = webhook.CanRetry(3);

        // Assert
        canRetry.ShouldBeTrue();
    }

    [Fact]
    public void CanRetry_ShouldReturnFalse_WhenRetryCountReachedMax()
    {
        // Arrange
        var webhook = WebhookEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "test.event", "{}", "https://example.com");
        webhook.MarkAsFailed();
        webhook.MarkAsFailed();
        webhook.MarkAsFailed();

        // Act
        var canRetry = webhook.CanRetry(3);

        // Assert
        canRetry.ShouldBeFalse();
    }
}
