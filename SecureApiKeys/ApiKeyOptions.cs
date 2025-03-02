using System.ComponentModel.DataAnnotations;

namespace SecureApiKeys;

/// <summary>
/// Configuration options for the SecureApiKeyGenerator.
/// </summary>
public class ApiKeyOptions
{
    /// <summary>
    /// Standard prefix identifying this as a secret key.
    /// Default: "sk" (similar to how Stripe uses 'sk' for secret keys)
    /// </summary>
    public string Prefix { get; set; } = "sk";

    /// <summary>
    /// Version string to include in the API key format.
    /// Default: "v1"
    /// </summary>
    public string Version { get; set; } = "v1";

    /// <summary>
    /// Number of bytes used for the main secret portion of the key.
    /// Default: 32 (256 bits of entropy)
    /// </summary>
    [Range(16, 64)]
    public int SecretBytes { get; set; } = 32;

    /// <summary>
    /// Number of bytes used to generate the unique prefix.
    /// Default: 4 (32 bits for the prefix)
    /// </summary>
    [Range(2, 16)]
    public int PrefixBytes { get; set; } = 4;

    /// <summary>
    /// Length of the unique prefix after Base64 encoding.
    /// Default: 6 characters
    /// </summary>
    [Range(4, 16)]
    public int UniqueIdLength { get; set; } = 6;

    /// <summary>
    /// Character used to replace '+' in Base64 encoding for URL safety.
    /// Default: '-' (following RFC 4648)
    /// </summary>
    public char PlusReplacement { get; set; } = '-';

    /// <summary>
    /// Character used to replace '/' in Base64 encoding for URL safety.
    /// Default: '.' (modified from standard '_' for easier reading)
    /// </summary>
    public char SlashReplacement { get; set; } = '.';

    /// <summary>
    /// Delimiter used between parts of the API key.
    /// Default: '_'
    /// </summary>
    public char Delimiter { get; set; } = '_';

    /// <summary>
    /// Validates that the options are in a consistent state.
    /// </summary>
    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(Prefix))
        {
            throw new ArgumentException("Prefix cannot be empty", nameof(Prefix));
        }

        if (string.IsNullOrWhiteSpace(Version))
        {
            throw new ArgumentException("Version cannot be empty", nameof(Version));
        }

        if (PrefixBytes < 2)
        {
            throw new ArgumentException("PrefixBytes must be at least 2", nameof(PrefixBytes));
        }

        if (SecretBytes < 16)
        {
            throw new ArgumentException("SecretBytes must be at least 16 (128 bits) for adequate security", nameof(SecretBytes));
        }

        if (UniqueIdLength < 4)
        {
            throw new ArgumentException("UniqueIdLength must be at least 4 characters", nameof(UniqueIdLength));
        }
    }
}