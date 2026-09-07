using System.Security.Cryptography;

namespace Eventarium.Providers.GitHub;

internal static class GitHubWebhookSignatureValidator
{
    private const string SignaturePrefix = "sha256=";
    private const int Sha256ByteCount = 32;

    public static bool IsValid(
        ReadOnlySpan<byte> payload,
        string? signature,
        ReadOnlySpan<byte> secret)
    {
        if (string.IsNullOrWhiteSpace(signature) ||
            !signature.StartsWith(SignaturePrefix, StringComparison.Ordinal) ||
            signature.Length != SignaturePrefix.Length + (Sha256ByteCount * 2))
        {
            return false;
        }

        byte[] suppliedHash;
        try
        {
            suppliedHash = Convert.FromHexString(signature[SignaturePrefix.Length..]);
        }
        catch (FormatException)
        {
            return false;
        }

        Span<byte> expectedHash = stackalloc byte[Sha256ByteCount];
        _ = HMACSHA256.HashData(secret, payload, expectedHash);
        return CryptographicOperations.FixedTimeEquals(expectedHash, suppliedHash);
    }
}
