using System;
using DocFlow.Inboxes;
using Shouldly;
using Xunit;

namespace DocFlow.Inboxes;

public class InboxTests
{
    [Fact]
    public void Create_ShouldCreateInbox_WithValidParameters()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var name = InboxName.Create("Test Inbox");
        var description = "Test Description";

        // Act
        var inbox = Inbox.Create(id, tenantId, name, description);

        // Assert
        inbox.ShouldNotBeNull();
        inbox.Id.ShouldBe(id);
        inbox.TenantId.ShouldBe(tenantId);
        inbox.Name.Value.ShouldBe("Test Inbox");
        inbox.Description.ShouldBe(description);
        inbox.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenNameIsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            Inbox.Create(id, tenantId, null!, "Description"));
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var inbox = Inbox.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            InboxName.Create("Old Name"), 
            "Old Description");
        
        var newName = InboxName.Create("New Name");
        var newDescription = "New Description";

        // Act
        inbox.UpdateDetails(newName, newDescription);

        // Assert
        inbox.Name.Value.ShouldBe("New Name");
        inbox.Description.ShouldBe(newDescription);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var inbox = Inbox.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            InboxName.Create("Test Inbox"), 
            null);

        // Act
        inbox.Deactivate();

        // Assert
        inbox.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var inbox = Inbox.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            InboxName.Create("Test Inbox"), 
            null);
        inbox.Deactivate();

        // Act
        inbox.Activate();

        // Assert
        inbox.IsActive.ShouldBeTrue();
    }
}
