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
    /// Default: '8' (modified from standard '-' for better visual consistency)
    /// </summary>
    public char PlusReplacement { get; set; } = '8';

    /// <summary>
    /// Character used to replace '/' in Base64 encoding for URL safety.
    /// Default: '9' (modified from standard '_' for better visual consistency)
    /// </summary>
    public char SlashReplacement { get; set; } = '9';

    /// <summary>
    /// Delimiter used between parts of the API key.
    /// Default: '_'
    /// </summary>
    public char Delimiter { get; set; } = '_';

    /// <summary>
    /// Validates that the options are in a consistent state.
    /// </summary>
    public void Validate()
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
        
        // Prevent using '+' as a replacement character or delimiter
        if (PlusReplacement == '+')
        {
            throw new ArgumentException("'+' cannot be used as a replacement character as it's a special character in Base64", nameof(PlusReplacement));
        }
        
        if (SlashReplacement == '+')
        {
            throw new ArgumentException("'+' cannot be used as a replacement character as it's a special character in Base64", nameof(SlashReplacement));
        }
        
        if (Delimiter == '+')
        {
            throw new ArgumentException("'+' cannot be used as a delimiter as it causes issues with string splitting", nameof(Delimiter));
        }
        
        // Ensure delimiter is different from replacement characters
        if (Delimiter == PlusReplacement)
        {
            throw new ArgumentException("Delimiter cannot be the same as PlusReplacement character", nameof(Delimiter));
        }
        
        if (Delimiter == SlashReplacement)
        {
            throw new ArgumentException("Delimiter cannot be the same as SlashReplacement character", nameof(Delimiter));
        }
    }
}