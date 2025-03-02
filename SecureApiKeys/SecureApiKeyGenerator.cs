using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace SecureApiKeys;

/// <summary>
/// Provides functionality for generating and validating secure API keys.
/// </summary>
public class SecureApiKeyGenerator
{
    private readonly ApiKeyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureApiKeyGenerator"/> class with default options.
    /// </summary>
    public SecureApiKeyGenerator() : this(new ApiKeyOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureApiKeyGenerator"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options to use for API key generation.</param>
    public SecureApiKeyGenerator(ApiKeyOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureApiKeyGenerator"/> class with options from the DI container.
    /// </summary>
    /// <param name="options">The options from the application configuration.</param>
    public SecureApiKeyGenerator(IOptions<ApiKeyOptions> options) : this(options?.Value ?? new ApiKeyOptions())
    {
    }

    /// <summary>
    /// Generates a cryptographically secure API key based on the configured options.
    /// </summary>
    /// <returns>A secure API key in the configured format.</returns>
    public string GenerateApiKey()
    {
        while (true)
        {
            // Generate random bytes for the unique ID prefix
            var prefixBytes = new byte[_options.PrefixBytes * 2]; // Generate more than we need for sufficient randomness
            RandomNumberGenerator.Fill(prefixBytes);

            // Convert to Base64, but make it URL-safe by replacing special characters
            var uniquePrefix = Convert.ToBase64String(prefixBytes)
                .Replace("+", _options.PlusReplacement.ToString())
                .Replace("/", _options.SlashReplacement.ToString())
                .TrimEnd('=');

            // Ensure we have enough characters for the unique ID
            if (uniquePrefix.Length < _options.UniqueIdLength) continue;

            // Take only the number of characters we need
            uniquePrefix = uniquePrefix[.._options.UniqueIdLength];

            // Generate the main secret portion
            var secretBytes = new byte[_options.SecretBytes];
            RandomNumberGenerator.Fill(secretBytes);

            var secret = Convert.ToBase64String(secretBytes)
                .Replace("+", _options.PlusReplacement.ToString())
                .Replace("/", _options.SlashReplacement.ToString())
                .TrimEnd('=');

            // Combine all parts using the configured delimiter
            return string.Join(_options.Delimiter, _options.Prefix, _options.Version, uniquePrefix, secret);
        }
    }

    /// <summary>
    /// Validates that an API key matches the expected format.
    /// This does not check if the key is actually valid in your system,
    /// only that it follows the correct format.
    /// </summary>
    /// <param name="apiKey">The API key to validate</param>
    /// <returns>True if the key matches the expected format, false otherwise</returns>
    public bool ValidateKeyFormat(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return false;

        var parts = apiKey.Split(_options.Delimiter);
        if (parts.Length != 4) return false;
        if (parts[0] != _options.Prefix) return false;
        if (parts[1] != _options.Version) return false;
        if (parts[2].Length != _options.UniqueIdLength) return false;

        // Minimum length check for security
        if (parts[3].Length < 16) return false;

        // Generate a test sample to determine the expected length
        var testBytes = new byte[_options.SecretBytes];
        var expectedLength = Convert.ToBase64String(testBytes)
            .Replace("+", _options.PlusReplacement.ToString())
            .Replace("/", _options.SlashReplacement.ToString())
            .TrimEnd('=')
            .Length;

        return parts[3].Length == expectedLength;
    }

    /// <summary>
    /// Generates multiple unique API keys in bulk.
    /// </summary>
    /// <param name="count">Number of unique API keys to generate</param>
    /// <returns>Array of secure API keys</returns>
    /// <exception cref="ArgumentException">Thrown when count is zero or negative</exception>
    public string[] GenerateMultipleApiKeys(int count)
    {
        if (count <= 0) throw new ArgumentException("Count must be positive", nameof(count));

        return Enumerable.Range(0, count)
            .Select(_ => GenerateApiKey())
            .ToArray();
    }

    /// <summary>
    /// Compares two API keys using a timing-safe comparison method to prevent timing attacks.
    /// </summary>
    /// <param name="knownKey">The known good API key (e.g., from configuration)</param>
    /// <param name="providedKey">The key to verify (e.g., from an API request)</param>
    /// <returns>True if the keys match exactly, false otherwise</returns>
    public static bool SecureCompare(string knownKey, string providedKey)
    {
        if (string.IsNullOrEmpty(knownKey) || string.IsNullOrEmpty(providedKey)) return false;

        var known = Encoding.UTF8.GetBytes(knownKey);
        var provided = Encoding.UTF8.GetBytes(providedKey);

        return known.Length == provided.Length &&
               CryptographicOperations.FixedTimeEquals(known, provided);
    }

    /// <summary>
    /// Creates a secure hash of an API key using SHA256.
    /// </summary>
    /// <param name="apiKey">The API key to hash</param>
    /// <returns>Base64-encoded SHA256 hash of the API key</returns>
    public static string HashApiKey(string? apiKey) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey ?? string.Empty)));
}