using System;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace DocFlow.Webhooks;

public interface IHmacSignatureService
{
    string GenerateSignature(string payload, string secret);
    bool VerifySignature(string payload, string signature, string secret);
}

public class HmacSignatureService : IHmacSignatureService, ITransientDependency
{
    public string GenerateSignature(string payload, string secret)
    {
        var encoding = new UTF8Encoding();
        var keyBytes = encoding.GetBytes(secret);
        var messageBytes = encoding.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifySignature(string payload, string signature, string secret)
    {
        var expectedSignature = GenerateSignature(payload, secret);
        return string.Equals(signature, expectedSignature, StringComparison.Ordinal);
    }
}
