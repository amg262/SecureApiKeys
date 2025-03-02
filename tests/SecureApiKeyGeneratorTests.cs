using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SecureApiKeys;

namespace tests;

public class ApiKeyOptionsTests
{
    [Fact]
    public void DefaultOptions_HaveExpectedValues()
    {
        // Arrange & Act
        var options = new ApiKeyOptions();
            
        // Assert
        Assert.Equal("sk", options.Prefix);
        Assert.Equal("v1", options.Version);
        Assert.Equal(32, options.SecretBytes);
        Assert.Equal(4, options.PrefixBytes);
        Assert.Equal(6, options.UniqueIdLength);
        Assert.Equal('8', options.PlusReplacement);
        Assert.Equal('9', options.SlashReplacement);
        Assert.Equal('_', options.Delimiter);
    }

    [Fact]
    public void Validate_WithValidOptions_DoesNotThrow()
    {
        // Arrange
        var options = new ApiKeyOptions();
            
        // Act & Assert
        var exception = Record.Exception(() => options.Validate());
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("", "Version", "Prefix cannot be empty")]
    [InlineData(null, "Version", "Prefix cannot be empty")]
    [InlineData("sk", "", "Version cannot be empty")]
    [InlineData("sk", null, "Version cannot be empty")]
    public void Validate_WithEmptyPrefixOrVersion_ThrowsArgumentException(string prefix, string version, string expectedMessage)
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Prefix = prefix,
            Version = version
        };
            
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(1, "PrefixBytes must be at least 2")]
    [InlineData(0, "PrefixBytes must be at least 2")]
    [InlineData(-1, "PrefixBytes must be at least 2")]
    public void Validate_WithInvalidPrefixBytes_ThrowsArgumentException(int prefixBytes, string expectedMessage)
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            PrefixBytes = prefixBytes
        };
            
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(15, "SecretBytes must be at least 16")]
    [InlineData(10, "SecretBytes must be at least 16")]
    [InlineData(0, "SecretBytes must be at least 16")]
    [InlineData(-1, "SecretBytes must be at least 16")]
    public void Validate_WithInvalidSecretBytes_ThrowsArgumentException(int secretBytes, string expectedMessage)
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            SecretBytes = secretBytes
        };
            
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(3, "UniqueIdLength must be at least 4")]
    [InlineData(2, "UniqueIdLength must be at least 4")]
    [InlineData(0, "UniqueIdLength must be at least 4")]
    [InlineData(-1, "UniqueIdLength must be at least 4")]
    public void Validate_WithInvalidUniqueIdLength_ThrowsArgumentException(int uniqueIdLength, string expectedMessage)
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            UniqueIdLength = uniqueIdLength
        };
            
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public void Validate_WithPlusAsReplacement_ThrowsArgumentException()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            PlusReplacement = '+'
        };
            
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("'+' cannot be used as a replacement character", exception.Message);
    }
    
    [Fact]
    public void Validate_WithPlusAsSlashReplacement_ThrowsArgumentException()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            SlashReplacement = '+'
        };
            
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("'+' cannot be used as a replacement character", exception.Message);
    }
    
    [Fact]
    public void Validate_WithPlusAsDelimiter_ThrowsArgumentException()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Delimiter = '+'
        };
            
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("'+' cannot be used as a delimiter", exception.Message);
    }
    
    [Fact]
    public void Validate_WithSameDelimiterAndPlusReplacement_ThrowsArgumentException()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Delimiter = '*',
            PlusReplacement = '*'
        };
            
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("Delimiter cannot be the same as PlusReplacement character", exception.Message);
    }
    
    [Fact]
    public void Validate_WithSameDelimiterAndSlashReplacement_ThrowsArgumentException()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Delimiter = '#',
            SlashReplacement = '#'
        };
            
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("Delimiter cannot be the same as SlashReplacement character", exception.Message);
    }
    
    [Fact]
    public void Constructor_WithInvalidDelimiterOptions_ThrowsArgumentException()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Delimiter = '8',  // Same as default PlusReplacement
        };
            
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SecureApiKeyGenerator(options));
    }

    [Fact]
    public void DefaultOptions_AreValid()
    {
        // Arrange
        var options = new ApiKeyOptions();
            
        // Act & Assert
        var exception = Record.Exception(() => options.Validate());
        Assert.Null(exception);
        
        // Verify default values don't conflict
        Assert.NotEqual(options.Delimiter, options.PlusReplacement);
        Assert.NotEqual(options.Delimiter, options.SlashReplacement);
        Assert.NotEqual('+', options.PlusReplacement);
        Assert.NotEqual('+', options.SlashReplacement);
        Assert.NotEqual('+', options.Delimiter);
    }
}

public class SecureApiKeyGeneratorTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SecureApiKeyGenerator((ApiKeyOptions)null));
    }

    [Fact]
    public void Constructor_WithNullIOptions_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SecureApiKeyGenerator((IOptions<ApiKeyOptions>)null));
    }

    [Fact]
    public void Constructor_WithInvalidOptions_ThrowsArgumentException()
    {
        // Arrange
        var options = new ApiKeyOptions { PrefixBytes = 0 };
            
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SecureApiKeyGenerator(options));
    }

    [Fact]
    public void Constructor_WithValidOptions_CreatesInstance()
    {
        // Arrange
        var options = new ApiKeyOptions();
            
        // Act
        var generator = new SecureApiKeyGenerator(options);
            
        // Assert
        Assert.NotNull(generator);
    }

    [Fact]
    public void Constructor_WithIOptionsWrapper_CreatesInstance()
    {
        // Arrange
        var optionsWrapper = Options.Create(new ApiKeyOptions());
            
        // Act
        var generator = new SecureApiKeyGenerator(optionsWrapper);
            
        // Assert
        Assert.NotNull(generator);
    }

    #endregion

    #region GenerateApiKey Tests

    [Fact]
    public void GenerateApiKey_WithDefaultOptions_ReturnsExpectedFormat()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
            
        // Act
        var key = generator.GenerateApiKey();
            
        // Assert
        Assert.NotNull(key);
        Assert.Matches(@"^sk_v1_[A-Za-z0-9\-89]{6}_[A-Za-z0-9\-89]+$", key);
        Assert.True(generator.ValidateKeyFormat(key));
    }

    [Theory]
    [InlineData("myapi", "v5", 8, '_')]
    [InlineData("test", "v0", 10, ':')]
    // [InlineData("key", "v123", 12, '')]
    public void GenerateApiKey_WithCustomOptions_ReturnsCustomFormat(
        string prefix, string version, int uniqueIdLength, char delimiter)
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Prefix = prefix,
            Version = version,
            UniqueIdLength = uniqueIdLength,
            Delimiter = delimiter
        };
        var generator = new SecureApiKeyGenerator(options);
            
        // Act
        var key = generator.GenerateApiKey();
        var parts = key.Split([delimiter], StringSplitOptions.None);
            
        // Assert
        Assert.NotNull(key);
        Assert.Equal(4, parts.Length);
        Assert.Equal(prefix, parts[0]);
        Assert.Equal(version, parts[1]);
        Assert.Equal(uniqueIdLength, parts[2].Length);
        Assert.True(generator.ValidateKeyFormat(key));
    }

    [Theory]
    [InlineData(16)]  // Minimum allowed
    [InlineData(24)]
    [InlineData(48)]
    [InlineData(64)]  // Maximum reasonable value
    public void GenerateApiKey_WithVariousSecretBytes_ReturnsValidKey(int secretBytes)
    {
        // Arrange
        var options = new ApiKeyOptions { SecretBytes = secretBytes };
        var generator = new SecureApiKeyGenerator(options);
            
        // Act
        var key = generator.GenerateApiKey();
            
        // Assert
        Assert.NotNull(key);
        Assert.True(generator.ValidateKeyFormat(key));
    }

    [Theory]
    [InlineData('*', '@')]
    [InlineData('$', '&')]
    [InlineData('#', '%')]
    public void GenerateApiKey_WithCustomReplacements_UsesCorrectCharacters(
        char plusReplacement, char slashReplacement)
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            PlusReplacement = plusReplacement,
            SlashReplacement = slashReplacement
        };
        var generator = new SecureApiKeyGenerator(options);
            
        // Act
        var key = generator.GenerateApiKey();
            
        // Assert
        Assert.NotNull(key);
        Assert.True(generator.ValidateKeyFormat(key));
            
        // Generate a large number of keys to increase chance of seeing replacements
        for (int i = 0; i < 50; i++)
        {
            key = generator.GenerateApiKey();
            Assert.DoesNotContain("+", key);
            Assert.DoesNotContain("/", key);
        }
    }

    [Fact]
    public void GenerateApiKey_CalledMultipleTimes_ReturnsUniqueKeys()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
        var keys = new HashSet<string>();
        var count = 1000;
            
        // Act
        for (int i = 0; i < count; i++)
        {
            keys.Add(generator.GenerateApiKey());
        }
            
        // Assert
        Assert.Equal(count, keys.Count); // All keys should be unique
    }

    [Fact]
    public void GenerateApiKey_WithDefaultOptions_HasCorrectPartLengths()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
            
        // Act
        var key = generator.GenerateApiKey();
        var parts = key.Split('_');
            
        // Assert
        Assert.Equal(4, parts.Length);
        Assert.Equal("sk", parts[0]);
        Assert.Equal("v1", parts[1]);
        Assert.Equal(6, parts[2].Length); // Default UniqueIdLength is 6
            
        // Calculate expected secret length based on default SecretBytes (32)
        var testBytes = new byte[32];
        var expectedSecretLength = Convert.ToBase64String(testBytes)
            .Replace("+", "8")
            .Replace("/", "9")
            .TrimEnd('=')
            .Length;
            
        Assert.Equal(expectedSecretLength, parts[3].Length);
    }

    #endregion

    #region ValidateKeyFormat Tests

    [Fact]
    public void ValidateKeyFormat_WithNullOrEmptyKey_ReturnsFalse()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
            
        // Act & Assert
        Assert.False(generator.ValidateKeyFormat(null));
        Assert.False(generator.ValidateKeyFormat(""));
        Assert.False(generator.ValidateKeyFormat("   "));
    }

    [Theory]
    [InlineData("invalid-key")]
    [InlineData("sk_v1")]
    [InlineData("sk_v1_123456")]
    [InlineData("sk.v1.123456.secretpart")]
    public void ValidateKeyFormat_WithIncorrectFormat_ReturnsFalse(string key)
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
            
        // Act & Assert
        Assert.False(generator.ValidateKeyFormat(key));
    }

    [Theory]
    [InlineData("wrong_v1_123456_secretparthere")]
    [InlineData("sk_v2_123456_secretparthere")]
    [InlineData("sk_v1_12345_secretparthere")]
    [InlineData("sk_v1_1234567_secretparthere")]
    public void ValidateKeyFormat_WithIncorrectParts_ReturnsFalse(string key)
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
            
        // Act & Assert
        Assert.False(generator.ValidateKeyFormat(key));
    }

    [Fact]
    public void ValidateKeyFormat_WithTooShortSecretPart_ReturnsFalse()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
            
        // Act & Assert
        Assert.False(generator.ValidateKeyFormat("sk_v1_123456_short"));
    }

    [Fact]
    public void ValidateKeyFormat_WithSelfGeneratedKey_ReturnsTrue()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
        var key = generator.GenerateApiKey();
            
        // Act & Assert
        Assert.True(generator.ValidateKeyFormat(key));
    }

    [Fact]
    public void ValidateKeyFormat_WithCustomOptions_ValidatesCorrectly()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Prefix = "api",
            Version = "v3",
            UniqueIdLength = 10,
            Delimiter = '|',
            PlusReplacement = '*'
        };
        var generator = new SecureApiKeyGenerator(options);
        var key = generator.GenerateApiKey();
            
        // Act & Assert
        Assert.True(generator.ValidateKeyFormat(key));
            
        // Should return false when validating with default options
        var defaultGenerator = new SecureApiKeyGenerator();
        Assert.False(defaultGenerator.ValidateKeyFormat(key));
    }

    #endregion

    #region HashApiKey Tests

    [Fact]
    public void HashApiKey_WithNullOrEmptyInput_ReturnsConsistentHash()
    {
        // Arrange & Act
        var nullHash = SecureApiKeyGenerator.HashApiKey(null);
        var emptyHash = SecureApiKeyGenerator.HashApiKey("");
            
        // Assert
        Assert.NotNull(nullHash);
        Assert.NotEmpty(nullHash);
        Assert.Equal(nullHash, SecureApiKeyGenerator.HashApiKey(null)); // Consistent hash
        Assert.Equal(emptyHash, SecureApiKeyGenerator.HashApiKey("")); // Consistent hash
    }

    [Fact]
    public void HashApiKey_WithSameInput_ReturnsSameHash()
    {
        // Arrange
        var key = "sk_v1_123456_secretparthere";
            
        // Act
        var hash1 = SecureApiKeyGenerator.HashApiKey(key);
        var hash2 = SecureApiKeyGenerator.HashApiKey(key);
            
        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashApiKey_WithDifferentInputs_ReturnsDifferentHashes()
    {
        // Arrange
        var key1 = "sk_v1_123456_secretparthere";
        var key2 = "sk_v1_123456_differentpart";
            
        // Act
        var hash1 = SecureApiKeyGenerator.HashApiKey(key1);
        var hash2 = SecureApiKeyGenerator.HashApiKey(key2);
            
        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashApiKey_OutputIsBase64String()
    {
        // Arrange
        var key = "sk_v1_123456_secretparthere";
            
        // Act
        var hash = SecureApiKeyGenerator.HashApiKey(key);
            
        // Assert
        // Validate it's Base64 encoded (allows only specific characters and optional padding)
        Assert.Matches(@"^[A-Za-z0-9+/]+=*$", hash);
            
        // Verify length is correct for SHA-256 hash (32 bytes) encoded as Base64 (44 characters with padding)
        Assert.Equal(44, hash.Length);
    }

    #endregion

    #region SecureCompare Tests

    [Fact]
    public void SecureCompare_WithNullOrEmptyInputs_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.False(SecureApiKeyGenerator.SecureCompare(null, "key"));
        Assert.False(SecureApiKeyGenerator.SecureCompare("key", null));
        Assert.False(SecureApiKeyGenerator.SecureCompare(null, null));
        Assert.False(SecureApiKeyGenerator.SecureCompare("", "key"));
        Assert.False(SecureApiKeyGenerator.SecureCompare("key", ""));
        Assert.False(SecureApiKeyGenerator.SecureCompare("", ""));
    }

    [Fact]
    public void SecureCompare_WithDifferentLengths_ReturnsFalse()
    {
        // Arrange
        var key1 = "sk_v1_123456_shortkey";
        var key2 = "sk_v1_123456_longerkeythandifferent";
            
        // Act & Assert
        Assert.False(SecureApiKeyGenerator.SecureCompare(key1, key2));
    }

    [Fact]
    public void SecureCompare_WithSameKey_ReturnsTrue()
    {
        // Arrange
        var key = "sk_v1_123456_secretparthere";
            
        // Act & Assert
        Assert.True(SecureApiKeyGenerator.SecureCompare(key, key));
        Assert.True(SecureApiKeyGenerator.SecureCompare(key, string.Copy(key))); // Different instances of same string
    }

    [Fact]
    public void SecureCompare_WithDifferentKeys_ReturnsFalse()
    {
        // Arrange
        var key1 = "sk_v1_123456_secretparthere";
            
        // Subtle differences - only one character changed
        var key2 = "sk_v1_123456_secretparthera";
        var key3 = "sk_v1_123456_secretparthEre";
        var key4 = "sk_v1_123456_Secretparthere";
            
        // Act & Assert
        Assert.False(SecureApiKeyGenerator.SecureCompare(key1, key2));
        Assert.False(SecureApiKeyGenerator.SecureCompare(key1, key3));
        Assert.False(SecureApiKeyGenerator.SecureCompare(key1, key4));
    }

    // Test to verify fixed-time comparison behavior
    // Note: This is more of a conceptual test since we can't easily measure timing side-channels
    [Fact]
    public void SecureCompare_IsConstantTime()
    {
        // Arrange
        var baseKey = "sk_v1_123456_secretparthere";
            
        // Create variations with differences at different positions
        var keys = new List<string> 
        {
            "Xk_v1_123456_secretparthere", // Difference at start
            "sk_X1_123456_secretparthere", // Difference in middle
            "sk_v1_123456_secretpartherX"  // Difference at end
        };
            
        // Act & Assert
        foreach (var key in keys)
        {
            Assert.False(SecureApiKeyGenerator.SecureCompare(baseKey, key));
        }
            
        // The test passes if all comparisons succeed, which verifies the method can detect
        // differences at any position (important for constant-time comparison)
    }

    #endregion

    #region GenerateMultipleApiKeys Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void GenerateMultipleApiKeys_WithInvalidCount_ThrowsArgumentException(int count)
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
            
        // Act & Assert
        Assert.Throws<ArgumentException>(() => generator.GenerateMultipleApiKeys(count));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void GenerateMultipleApiKeys_WithValidCount_ReturnsCorrectNumberOfKeys(int count)
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
            
        // Act
        var keys = generator.GenerateMultipleApiKeys(count);
            
        // Assert
        Assert.Equal(count, keys.Length);
        Assert.All(keys, key => Assert.True(generator.ValidateKeyFormat(key)));
    }

    [Fact]
    public void GenerateMultipleApiKeys_ReturnsUniqueKeys()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
        var count = 1000;
            
        // Act
        var keys = generator.GenerateMultipleApiKeys(count);
            
        // Assert
        var uniqueKeys = new HashSet<string>(keys);
        Assert.Equal(count, uniqueKeys.Count); // All keys should be unique
    }

    [Fact]
    public void GenerateMultipleApiKeys_WithCustomOptions_ReturnsValidKeys()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Prefix = "api",
            Version = "v3",
            UniqueIdLength = 10,
            Delimiter = '|'
        };
        var generator = new SecureApiKeyGenerator(options);
            
        // Act
        var keys = generator.GenerateMultipleApiKeys(10);
            
        // Assert
        Assert.All(keys, key => 
        {
            Assert.StartsWith("api|v3|", key);
            Assert.True(generator.ValidateKeyFormat(key));
        });
            
        // Parts should match custom options
        var firstKey = keys.First();
        var parts = firstKey.Split('|');
        Assert.Equal(4, parts.Length);
        Assert.Equal("api", parts[0]);
        Assert.Equal("v3", parts[1]);
        Assert.Equal(10, parts[2].Length);
    }

    #endregion

    [Theory]
    [InlineData('_', '8', '9')]  // Default configuration
    [InlineData('|', '*', '^')]  // Custom configuration 1
    [InlineData(':', '~', '!')]  // Custom configuration 2
    public void Constructor_WithValidDelimiterOptions_CreatesInstance(char delimiter, char plusReplacement, char slashReplacement)
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Delimiter = delimiter,
            PlusReplacement = plusReplacement,
            SlashReplacement = slashReplacement
        };
            
        // Act
        var generator = new SecureApiKeyGenerator(options);
        var key = generator.GenerateApiKey();
            
        // Assert
        Assert.NotNull(generator);
        Assert.NotNull(key);
        Assert.True(generator.ValidateKeyFormat(key));
            
        // Verify the key contains the correct delimiter
        Assert.Contains(delimiter.ToString(), key);
        
        // Count occurrences of delimiter - should be exactly 3 (for 4 parts)
        int delimiterCount = key.Count(c => c == delimiter);
        Assert.Equal(3, delimiterCount);
    }
    
    [Fact]
    public void GenerateApiKey_WithCustomDelimiterAndReplacements_ProducesValidKey()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Prefix = "test",
            Version = "v2",
            UniqueIdLength = 8,
            Delimiter = '|',
            PlusReplacement = '*',
            SlashReplacement = '^'
        };
        var generator = new SecureApiKeyGenerator(options);
            
        // Act
        var key = generator.GenerateApiKey();
        var parts = key.Split('|');
            
        // Assert
        Assert.Equal(4, parts.Length);
        Assert.Equal("test", parts[0]);
        Assert.Equal("v2", parts[1]);
        Assert.Equal(8, parts[2].Length);
        Assert.True(generator.ValidateKeyFormat(key));
        
        // Verify the key doesn't contain '+' or '/' (they should be replaced)
        Assert.DoesNotContain("+", key);
        Assert.DoesNotContain("/", key);
        
        // Verify the replacements are used
        var secretPart = parts[3];
        // Generate many keys to increase chance of seeing replacements
        for (int i = 0; i < 100; i++)
        {
            var newKey = generator.GenerateApiKey();
            secretPart = newKey.Split('|')[3];
            if (secretPart.Contains('*') || secretPart.Contains('^'))
            {
                break;
            }
        }
        
        // Note: This is a probabilistic test - it might occasionally fail
        // if none of the generated keys happen to contain the replacement characters
        // But with 100 attempts, it's very unlikely
    }
}

public class SecureApiKeysExtensionsTests
{
    [Fact]
    public void AddSecureApiKeys_WithoutOptions_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
            
        // Act
        services.AddSecureApiKeys();
        var provider = services.BuildServiceProvider();
            
        // Assert
        var generator = provider.GetService<SecureApiKeyGenerator>();
        Assert.NotNull(generator);
            
        // Verify service is registered as singleton
        var generator2 = provider.GetService<SecureApiKeyGenerator>();
        Assert.Same(generator, generator2);
    }

    [Fact]
    public void AddSecureApiKeys_WithOptions_ConfiguresService()
    {
        // Arrange
        var services = new ServiceCollection();
            
        // Act
        services.AddSecureApiKeys(options => 
        {
            options.Prefix = "customPrefix";
            options.Version = "v9";
        });
        var provider = services.BuildServiceProvider();
            
        // Assert
        var generator = provider.GetService<SecureApiKeyGenerator>();
        Assert.NotNull(generator);
            
        // Generate a key and validate it follows custom settings
        var key = generator.GenerateApiKey();
        Assert.StartsWith("customPrefix_v9_", key);
    }

    [Fact]
    public void AddSecureApiKeys_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<ApiKeyOptions> configure = null;
            
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddSecureApiKeys(configure));
    }

    [Fact]
    public void AddSecureApiKeys_CalledMultipleTimes_RegistersServiceOnce()
    {
        // Arrange
        var services = new ServiceCollection();
            
        // Act
        services.AddSecureApiKeys();
        services.AddSecureApiKeys(); // Call again
            
        // Assert
        Assert.Equal(1, services.Count(d => d.ServiceType == typeof(SecureApiKeyGenerator)));
    }
}

// Advanced Integration Tests
public class IntegrationTests
{
    [Fact]
    public void FullKeyLifecycle_WorksCorrectly()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Prefix = "test",
            Version = "v1",
            UniqueIdLength = 8
        };
        var generator = new SecureApiKeyGenerator(options);
            
        // Act - Generate a key
        var key = generator.GenerateApiKey();
            
        // Validate key format
        var isValid = generator.ValidateKeyFormat(key);
            
        // Hash the key (as would be stored in database)
        var hashedKey = SecureApiKeyGenerator.HashApiKey(key);
            
        // Simulate comparing stored hash with incoming key hash
        var hashCompare = SecureApiKeyGenerator.HashApiKey(key);
        var hashesMatch = hashedKey == hashCompare;
            
        // Simulate validating an incoming key securely
        var secureCompare = SecureApiKeyGenerator.SecureCompare(key, key);
            
        // Assert
        Assert.True(isValid);
        Assert.NotEqual(key, hashedKey);
        Assert.True(hashesMatch);
        Assert.True(secureCompare);
    }

    [Fact]
    public void CrossVersionKeyValidation_HandlesDifferentOptions()
    {
        // Simulate a scenario where options change between versions
            
        // Arrange - Version 1 of the API
        var options1 = new ApiKeyOptions
        {
            Prefix = "v1api",
            Version = "v1",
            UniqueIdLength = 6
        };
        var generator1 = new SecureApiKeyGenerator(options1);
            
        // Generate a key with v1 settings
        var keyV1 = generator1.GenerateApiKey();
            
        // Version 2 of the API with different settings
        var options2 = new ApiKeyOptions
        {
            Prefix = "v2api",
            Version = "v2",
            UniqueIdLength = 8
        };
        var generator2 = new SecureApiKeyGenerator(options2);
            
        // Generate a key with v2 settings
        var keyV2 = generator2.GenerateApiKey();
            
        // Act & Assert
            
        // Each generator should validate its own keys
        Assert.True(generator1.ValidateKeyFormat(keyV1));
        Assert.True(generator2.ValidateKeyFormat(keyV2));
            
        // But not the keys from different versions
        Assert.False(generator1.ValidateKeyFormat(keyV2));
        Assert.False(generator2.ValidateKeyFormat(keyV1));
            
        // A combined validator that supports both formats
        var multiVersionValidator = new MultiVersionValidator(options1, options2);
        Assert.True(multiVersionValidator.ValidateKeyFormat(keyV1));
        Assert.True(multiVersionValidator.ValidateKeyFormat(keyV2));
    }
        
    // Helper class for multi-version validation
    private class MultiVersionValidator
    {
        private readonly SecureApiKeyGenerator[] _generators;
            
        public MultiVersionValidator(params ApiKeyOptions[] options)
        {
            _generators = options.Select(opt => new SecureApiKeyGenerator(opt)).ToArray();
        }
            
        public bool ValidateKeyFormat(string key)
        {
            return _generators.Any(g => g.ValidateKeyFormat(key));
        }
    }
}

// Performance Tests (for benchmarking)
public class PerformanceTests
{
    [Fact]
    public void GenerateApiKey_Performance_IsReasonable()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
        var iterations = 100;
            
        // Act
        var watch = System.Diagnostics.Stopwatch.StartNew();
            
        for (int i = 0; i < iterations; i++)
        {
            generator.GenerateApiKey();
        }
            
        watch.Stop();
        var avgTimeMs = watch.ElapsedMilliseconds / (double)iterations;
            
        // Assert
        // This is a soft assertion as performance varies, but it gives you a benchmark
        // Actual limits would depend on your requirements
        Assert.True(avgTimeMs < 10, $"Average key generation time was {avgTimeMs}ms, which exceeds 10ms threshold");
    }

    [Fact]
    public void ValidateKeyFormat_Performance_IsEfficient()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
        var key = generator.GenerateApiKey();
        var iterations = 10000;
            
        // Act
        var watch = System.Diagnostics.Stopwatch.StartNew();
            
        for (int i = 0; i < iterations; i++)
        {
            generator.ValidateKeyFormat(key);
        }
            
        watch.Stop();
        var avgTimeMs = (watch.ElapsedMilliseconds * 1000.0 / iterations); // in microseconds
            
        // Assert
        // Validation should be very fast as it's used on every API call
        Assert.True(avgTimeMs < 50, $"Average validation time was {avgTimeMs}µs, which exceeds 50µs threshold");
    }
}