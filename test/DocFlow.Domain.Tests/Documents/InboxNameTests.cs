using System;
using DocFlow.Documents;
using Shouldly;
using Xunit;

namespace DocFlow.Domain.Tests.Documents;

/// <summary>
/// Unit tests for InboxName value object.
/// </summary>
public sealed class InboxNameTests
{
    [Fact]
    public void Create_WithValidName_ShouldSucceed()
    {
        // Arrange
        var validName = "Accounting";

        // Act
        var inbox = InboxName.Create(validName);

        // Assert
        inbox.ShouldNotBeNull();
        inbox.Value.ShouldBe(validName);
    }

    [Fact]
    public void Create_WithNameContainingSpaces_ShouldTrim()
    {
        // Arrange
        var nameWithSpaces = "  Legal Department  ";

        // Act
        var inbox = InboxName.Create(nameWithSpaces);

        // Assert
        inbox.Value.ShouldBe("Legal Department");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespace_ShouldThrowArgumentException(string invalidName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => InboxName.Create(invalidName))
            .Message.ShouldContain("Inbox name cannot be empty");
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('A', 101); // 101 characters (max is 100)

        // Act & Assert
        Should.Throw<ArgumentException>(() => InboxName.Create(longName))
            .Message.ShouldContain("cannot exceed 100 characters");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var inbox = InboxName.Create("HR");

        // Act
        var result = inbox.ToString();

        // Assert
        result.ShouldBe("HR");
    }

    [Fact]
    public void ValueEquality_SameValue_ShouldBeEqual()
    {
        // Arrange
        var inbox1 = InboxName.Create("Finance");
        var inbox2 = InboxName.Create("Finance");

        // Act & Assert
        inbox1.ShouldBe(inbox2);
        inbox1.Equals(inbox2).ShouldBeTrue();
        (inbox1 == inbox2).ShouldBeTrue();
    }

    [Fact]
    public void ValueEquality_DifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var inbox1 = InboxName.Create("Finance");
        var inbox2 = InboxName.Create("Legal");

        // Act & Assert
        inbox1.ShouldNotBe(inbox2);
        inbox1.Equals(inbox2).ShouldBeFalse();
        (inbox1 != inbox2).ShouldBeTrue();
    }
}
