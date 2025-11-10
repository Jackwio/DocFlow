using DocFlow.Webhooks;
using Shouldly;
using Xunit;

namespace DocFlow.Domain.Tests.Webhooks;

public class HmacSignatureServiceTests
{
    private readonly HmacSignatureService _hmacSignatureService;

    public HmacSignatureServiceTests()
    {
        _hmacSignatureService = new HmacSignatureService();
    }

    [Fact]
    public void GenerateSignature_ShouldReturnSignature()
    {
        // Arrange
        var payload = "{\"test\":\"data\"}";
        var secret = "my-secret-key";

        // Act
        var signature = _hmacSignatureService.GenerateSignature(payload, secret);

        // Assert
        signature.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateSignature_ShouldReturnSameSignature_ForSameInput()
    {
        // Arrange
        var payload = "{\"test\":\"data\"}";
        var secret = "my-secret-key";

        // Act
        var signature1 = _hmacSignatureService.GenerateSignature(payload, secret);
        var signature2 = _hmacSignatureService.GenerateSignature(payload, secret);

        // Assert
        signature1.ShouldBe(signature2);
    }

    [Fact]
    public void GenerateSignature_ShouldReturnDifferentSignature_ForDifferentPayload()
    {
        // Arrange
        var payload1 = "{\"test\":\"data1\"}";
        var payload2 = "{\"test\":\"data2\"}";
        var secret = "my-secret-key";

        // Act
        var signature1 = _hmacSignatureService.GenerateSignature(payload1, secret);
        var signature2 = _hmacSignatureService.GenerateSignature(payload2, secret);

        // Assert
        signature1.ShouldNotBe(signature2);
    }

    [Fact]
    public void GenerateSignature_ShouldReturnDifferentSignature_ForDifferentSecret()
    {
        // Arrange
        var payload = "{\"test\":\"data\"}";
        var secret1 = "my-secret-key-1";
        var secret2 = "my-secret-key-2";

        // Act
        var signature1 = _hmacSignatureService.GenerateSignature(payload, secret1);
        var signature2 = _hmacSignatureService.GenerateSignature(payload, secret2);

        // Assert
        signature1.ShouldNotBe(signature2);
    }

    [Fact]
    public void VerifySignature_ShouldReturnTrue_ForValidSignature()
    {
        // Arrange
        var payload = "{\"test\":\"data\"}";
        var secret = "my-secret-key";
        var signature = _hmacSignatureService.GenerateSignature(payload, secret);

        // Act
        var isValid = _hmacSignatureService.VerifySignature(payload, signature, secret);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void VerifySignature_ShouldReturnFalse_ForInvalidSignature()
    {
        // Arrange
        var payload = "{\"test\":\"data\"}";
        var secret = "my-secret-key";
        var invalidSignature = "invalid-signature";

        // Act
        var isValid = _hmacSignatureService.VerifySignature(payload, invalidSignature, secret);

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void VerifySignature_ShouldReturnFalse_WhenPayloadModified()
    {
        // Arrange
        var originalPayload = "{\"test\":\"data\"}";
        var modifiedPayload = "{\"test\":\"modified\"}";
        var secret = "my-secret-key";
        var signature = _hmacSignatureService.GenerateSignature(originalPayload, secret);

        // Act
        var isValid = _hmacSignatureService.VerifySignature(modifiedPayload, signature, secret);

        // Assert
        isValid.ShouldBeFalse();
    }
}
