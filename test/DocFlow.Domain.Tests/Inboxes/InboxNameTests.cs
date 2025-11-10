using System;
using DocFlow.Inboxes;
using Shouldly;
using Xunit;

namespace DocFlow.Inboxes;

public class InboxNameTests
{
    [Fact]
    public void Create_ShouldCreateInboxName_WithValidName()
    {
        // Arrange & Act
        var inboxName = InboxName.Create("Test Inbox");

        // Assert
        inboxName.ShouldNotBeNull();
        inboxName.Value.ShouldBe("Test Inbox");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var inboxName = InboxName.Create("  Test Inbox  ");

        // Assert
        inboxName.Value.ShouldBe("Test Inbox");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Create_ShouldThrowException_WhenNameIsNullOrWhitespace(string? name)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => InboxName.Create(name));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenNameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act & Assert
        Should.Throw<ArgumentException>(() => InboxName.Create(longName));
    }

    [Fact]
    public void ValueObjectEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var name1 = InboxName.Create("Test");
        var name2 = InboxName.Create("Test");
        var name3 = InboxName.Create("Different");

        // Assert - Compare values instead of objects
        name1.Value.ShouldBe(name2.Value);
        name1.Value.ShouldNotBe(name3.Value);
    }
}
