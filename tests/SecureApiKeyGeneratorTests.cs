using SecureApiKeys;

namespace tests;

public class SecureApiKeyGeneratorTests
{
    [Fact]
    public void GenerateApiKey_WithDefaultOptions_ReturnsValidKey()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();

        // Act
        var key = generator.GenerateApiKey();

        var bo = generator.ValidateKeyFormat(key);
        
        // Assert
        Assert.NotNull(key);
        Assert.True(generator.ValidateKeyFormat(key));
        Assert.StartsWith("sk_v1_", key);

        var parts = key.Split('_');
        Assert.Equal(4, parts.Length);
        Assert.Equal("sk", parts[0]);
        Assert.Equal("v1", parts[1]);
        Assert.Equal(6, parts[2].Length);
    }

    [Fact]
    public void GenerateApiKey_WithCustomOptions_ReturnsCustomFormattedKey()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            Prefix = "api",
            Version = "v2",
            UniqueIdLength = 8,
            Delimiter = '-'
        };
        var generator = new SecureApiKeyGenerator(options);

        // Act
        var key = generator.GenerateApiKey();

        // Assert
        Assert.NotNull(key);
        Assert.True(generator.ValidateKeyFormat(key));
        Assert.StartsWith("api-v2-", key);

        var parts = key.Split('-');
        Assert.Equal(4, parts.Length);
        Assert.Equal("api", parts[0]);
        Assert.Equal("v2", parts[1]);
        Assert.Equal(8, parts[2].Length);
    }

    [Fact]
    public void ValidateKeyFormat_WithInvalidKey_ReturnsFalse()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();

        // Act & Assert
        Assert.False(generator.ValidateKeyFormat(null));
        Assert.False(generator.ValidateKeyFormat(""));
        Assert.False(generator.ValidateKeyFormat("invalid-key"));
        Assert.False(generator.ValidateKeyFormat("sk_v1_tooshort"));
        Assert.False(generator.ValidateKeyFormat("wrong_v1_123456_secretpart"));
        Assert.False(generator.ValidateKeyFormat("sk_v2_123456_secretpart")); // Wrong version
    }

    [Fact]
    public void HashApiKey_WithValidKey_ReturnsHashedValue()
    {
        // Arrange
        var key = "sk_v1_123456_secretparthere";

        // Act
        var hash = SecureApiKeyGenerator.HashApiKey(key);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEqual(key, hash);

        // Hash should be consistent
        var hash2 = SecureApiKeyGenerator.HashApiKey(key);
        Assert.Equal(hash, hash2);
    }

    [Fact]
    public void SecureCompare_WithMatchingKeys_ReturnsTrue()
    {
        // Arrange
        var key1 = "sk_v1_123456_secretparthere";
        var key2 = "sk_v1_123456_secretparthere";

        // Act
        var result = SecureApiKeyGenerator.SecureCompare(key1, key2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SecureCompare_WithDifferentKeys_ReturnsFalse()
    {
        // Arrange
        var key1 = "sk_v1_123456_secretparthere";
        var key2 = "sk_v1_123456_differentpart";

        // Act
        var result = SecureApiKeyGenerator.SecureCompare(key1, key2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateMultipleApiKeys_ReturnsSpecifiedNumberOfKeys()
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();
        var count = 5;

        // Act
        var keys = generator.GenerateMultipleApiKeys(count);

        // Assert
        Assert.Equal(count, keys.Length);

        // All keys should be valid and unique
        var uniqueKeys = new HashSet<string>();
        foreach (var key in keys)
        {
            Assert.True(generator.ValidateKeyFormat(key));
            Assert.True(uniqueKeys.Add(key)); // Should be unique
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerateMultipleApiKeys_WithInvalidCount_ThrowsArgumentException(int count)
    {
        // Arrange
        var generator = new SecureApiKeyGenerator();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => generator.GenerateMultipleApiKeys(count));
    }
}