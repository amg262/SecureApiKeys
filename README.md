# 🔐 SecureApiKeys

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)
![Version](https://img.shields.io/badge/version-1.0.3-brightgreen)

![NuGet](https://img.shields.io/nuget/v/SecureApiKeys)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/amg262/SecureApiKeys/publish.yml)

A robust, production-ready .NET library for generating cryptographically secure API keys with customizable formatting. Built with security best practices and flexibility in mind.

## 📋 Overview

Managing API keys securely is a critical aspect of application security. SecureApiKeys provides a complete solution for generating, validating, and managing API keys in .NET applications with industry-standard security practices.

### Why SecureApiKeys?

- **Cryptographically Secure**: Uses .NET's `RandomNumberGenerator` for true cryptographic randomness
- **Highly Customizable**: Configure every aspect of your API key format (prefix, length, encoding)
- **Security Focused**: Includes timing-safe comparison and secure hashing for key storage
- **DI Integration**: Seamless integration with .NET Dependency Injection
- **Production Ready**: Comprehensive test coverage and performance optimization
- **Modern .NET**: Built with .NET 9.0 using the latest language features

## ⚙️ Installation

Install the package from NuGet:

```bash
# Using the .NET CLI
dotnet add package SecureApiKeys

# Using Package Manager Console
Install-Package SecureApiKeys
```

## 🚀 Quick Start

### Basic Usage

```csharp
// Create with default options - produces keys like "sk_v1_abc123_ABCdef8ghIJklm9"
var generator = new SecureApiKeyGenerator();

// Generate a single API key
string apiKey = generator.GenerateApiKey();

// Validate key format
bool isValid = generator.ValidateKeyFormat(apiKey);

// Hash a key for secure storage in a database
string hashedKey = SecureApiKeyGenerator.HashApiKey(apiKey);

// Securely compare keys (timing-safe comparison prevents timing attacks)
bool matches = SecureApiKeyGenerator.SecureCompare(knownKey, providedKey);
```

### Dependency Injection

```csharp
// In Program.cs or Startup.cs
builder.Services.AddSecureApiKeys(options =>
{
    options.Prefix = "api";   // Default: "sk"
    options.Version = "v2";   // Default: "v1"
});

// Then inject it in your services
public class ApiKeyService
{
    private readonly SecureApiKeyGenerator _keyGenerator;

    public ApiKeyService(SecureApiKeyGenerator keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    public string CreateNewApiKey()
    {
        return _keyGenerator.GenerateApiKey();
    }
}
```

## 🔧 Key Customization

Every aspect of the API key format can be customized:

```csharp
var options = new ApiKeyOptions
{
    // Identifier prefixes (default: "sk")
    Prefix = "api",
    
    // Version string (default: "v1")
    Version = "v2",
    
    // Security strength - 32 bytes = 256 bits of entropy (default: 32)
    SecretBytes = 48,  // Increase to 384 bits for even stronger keys
    
    // Unique identifier length (default: 6)
    UniqueIdLength = 8,
    
    // Delimiter character between sections (default: '_')
    Delimiter = '-',
    
    // Base64 URL-safe character replacements (default: '8' and '9')
    PlusReplacement = '_',
    SlashReplacement = '~'
};

var generator = new SecureApiKeyGenerator(options);

// Creates key like: "api-v2-a1b2c3d4-eRt5TyUiOp..."
string apiKey = generator.GenerateApiKey();
```

## 🔍 API Key Structure

By default, the library generates keys in this format:

```
sk_v1_abc123_eRt5TyUiOpAsDfGhJkLzXcVbNm12345...
│  │   │       └─ Secret (cryptographically secure random data)
│  │   │
│  │   └─ Unique Identifier (helps with key identification)
│  │
│  └─ Version (allows for future format changes)
│
└─ Prefix (identifies key type, e.g., "sk" for secret key)
```

For example, a generated key might look like: `sk_v1_123456_ABCdef8ghIJklm9`

This structure follows industry best practices:
- **Prefix**: Identifies the key type (e.g., Stripe uses "sk_" for secret keys, "pk_" for publishable keys)
- **Version**: Allows for changes to the key format in the future without breaking compatibility
- **Unique ID**: Makes keys more human-readable and helps with key identification
- **Secret**: The cryptographically secure random portion providing the security

## 🛡️ Security Best Practices

### Never Store Raw API Keys

Always store the hashed version of API keys in your database:

```csharp
// When generating a new key for a user
string apiKey = _keyGenerator.GenerateApiKey();
string hashedKey = SecureApiKeyGenerator.HashApiKey(apiKey);

// Store hashedKey in your database
await _dbContext.ApiKeys.AddAsync(new ApiKey
{
    UserId = userId,
    HashedKey = hashedKey,
    CreatedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddDays(90)
});

// Return the raw key to the user (this is their only chance to see it)
return apiKey;
```

### Timing-Safe Comparison

Always use `SecureCompare` to prevent timing attacks when validating API keys:

```csharp
// When validating an incoming API key
public async Task<bool> ValidateApiKey(string providedKey)
{
    // Get the stored hashed key from database
    string storedHashedKey = await _dbContext.ApiKeys
        .Where(k => k.UserId == userId && k.ExpiresAt > DateTime.UtcNow)
        .Select(k => k.HashedKey)
        .FirstOrDefaultAsync();
        
    if (storedHashedKey == null)
        return false;
        
    // Hash the provided key
    string hashedProvidedKey = SecureApiKeyGenerator.HashApiKey(providedKey);
    
    // Compare using timing-safe comparison
    return SecureApiKeyGenerator.SecureCompare(storedHashedKey, hashedProvidedKey);
}
```

### Key Rotation and Revocation

Implement key rotation and revocation strategies:

```csharp
// Sample key rotation implementation
public async Task<string> RotateApiKey(string userId)
{
    // Expire the old key
    var oldKey = await _dbContext.ApiKeys
        .Where(k => k.UserId == userId && k.ExpiresAt > DateTime.UtcNow)
        .FirstOrDefaultAsync();
        
    if (oldKey != null)
    {
        oldKey.ExpiresAt = DateTime.UtcNow;
        oldKey.Revoked = true;
    }
    
    // Generate new key
    string newKey = _keyGenerator.GenerateApiKey();
    string hashedNewKey = SecureApiKeyGenerator.HashApiKey(newKey);
    
    // Store the new key
    await _dbContext.ApiKeys.AddAsync(new ApiKey
    {
        UserId = userId,
        HashedKey = hashedNewKey,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(90)
    });
    
    await _dbContext.SaveChangesAsync();
    
    return newKey;
}
```

## 📊 Bulk Operations

Generate multiple API keys in a single operation:

```csharp
// Generate 100 API keys (e.g., for initial system setup)
string[] keys = _keyGenerator.GenerateMultipleApiKeys(100);

// Generate keys with their hashes (for storage)
var keyPairs = keys.Select(key => new 
{
    ApiKey = key,
    HashedKey = SecureApiKeyGenerator.HashApiKey(key)
}).ToList();
```

## 🌟 Advanced Usage Scenarios

### Custom Key Format for Different Environments

```csharp
// Different key formats for different environments
services.AddSecureApiKeys(options =>
{
    if (_environment.IsDevelopment())
    {
        options.Prefix = "dev";
        options.Version = "test";
    }
    else if (_environment.IsStaging())
    {
        options.Prefix = "stg";
    }
    else 
    {
        options.Prefix = "prod";
    }
});
```

### Different Key Types

```csharp
// Create generators for different key types
services.AddSingleton("admin-keys", new SecureApiKeyGenerator(new ApiKeyOptions 
{ 
    Prefix = "admin", 
    SecretBytes = 48  // Stronger keys for admins
}));

services.AddSingleton("readonly-keys", new SecureApiKeyGenerator(new ApiKeyOptions 
{ 
    Prefix = "ro", 
    SecretBytes = 24  // Shorter keys for readonly access
}));

// Use with constructor injection
public class KeyManagementService
{
    private readonly SecureApiKeyGenerator _adminKeyGenerator;
    private readonly SecureApiKeyGenerator _readonlyKeyGenerator;
    
    public KeyManagementService(
        [FromKeyedServices("admin-keys")] SecureApiKeyGenerator adminKeyGenerator,
        [FromKeyedServices("readonly-keys")] SecureApiKeyGenerator readonlyKeyGenerator)
    {
        _adminKeyGenerator = adminKeyGenerator;
        _readonlyKeyGenerator = readonlyKeyGenerator;
    }
    
    // Use different generators based on key type
}
```

## 🎮 Sample Application

The library includes a complete sample console application that demonstrates all the key features and use cases. When run, it outputs detailed information about different key configurations, generated examples, and security features.

### Running the Sample

```bash
# Navigate to the sample directory
cd sample

# Run the sample
dotnet run
```

### Sample Program Output

When you run the sample application, you'll see the following comprehensive demonstration of various API key configurations and utilities:

```
SecureApiKeys Sample Application
================================

1. Default API Key Configuration
----------------------------------
Configuration:
  • Prefix: "sk"
  • Version: "v1"
  • Secret Bytes: 32
  • Prefix Bytes: 4
  • Unique ID Length: 6
  • Delimiter: '_'
  • Plus Replacement: '8'
  • Slash Replacement: '9'

Generated Key:
  sk_v1_b2ZpY1_RFNMbTNLVzRyM3ZOVHVvUlZ1MkZiYmxNYW9ZNGVSclV3T1JOYg

Key Structure:
  Prefix:    sk
  Version:   v1
  Unique ID: b2ZpY1
  Secret:    RFNMbTNLVzRyM3ZOVHVvUlZ1MkZiYmxNYW9ZNGVSclV3T1JOYg

Estimated Entropy: ~252.0 bits

Multiple Key Generation (3 keys):
  sk_v1_aGV0eU_RjlmRVhqdlJuY1BDYnFCNEF1ZmdXVkhQdVhlQXVyQW84ZVpSeg
  sk_v1_ODRseG_UGFpZ05qS3htRmFrV3pZaVl3SHF2U2NqY2ZSc2RsbVViSXdkZw
  sk_v1_NnZvZG_MTU3ZExPb0V5UlJ0YXhiQk9UY2RJd3NUQzJGU1ZqWmJRRE5KZw

2. Stripe-like API Key Configuration
------------------------------------
Configuration:
  • Prefix: "sk_test"
  • Version: "v1"
  • Secret Bytes: 24
  • Prefix Bytes: 4
  • Unique ID Length: 8
  • Delimiter: '_'
  • Plus Replacement: '8'
  • Slash Replacement: '9'

Generated Key:
  sk_test_v1_TkVzMUxSQW9_YVNyU2g0bWdxWU5ZNWVLbEtkbVVFcw

Key Structure:
  Prefix:    sk_test
  Version:   v1
  Unique ID: TkVzMUxSQW9
  Secret:    YVNyU2g0bWdxWU5ZNWVLbEtkbVVFcw

Estimated Entropy: ~207.4 bits

Multiple Key Generation (3 keys):
  sk_test_v1_Y1ZaWnB2VGU_d1c0TG9mQ0d0MmIwZnZiWlJYUzlZdzQ
  sk_test_v1_eXVBQmRIaWk_YzNlM0lRUXBNTXlnb0FQdm9YNTBJbXM
  sk_test_v1_NXBuMXJaT2E_dG1TZ051d2FzU1dmZG1DVmdKWDJpekw

3. GitHub PAT-like Token Configuration
---------------------------------------
Configuration:
  • Prefix: "ghp"
  • Version: "v1"
  • Secret Bytes: 36
  • Prefix Bytes: 4
  • Unique ID Length: 10
  • Delimiter: '_'
  • Plus Replacement: 'A'
  • Slash Replacement: 'B'

Generated Key:
  ghp_v1_dXJZaGVUQnYx_1pBbjFXeGU0dm5TMTNwUGdoVnRiZ1JKcGFVUmR0VjZBOUxJaDE

Key Structure:
  Prefix:    ghp
  Version:   v1
  Unique ID: dXJZaGVUQnYx
  Secret:    1pBbjFXeGU0dm5TMTNwUGdoVnRiZ1JKcGFVUmR0VjZBOUxJaDE

Estimated Entropy: ~297.8 bits

Multiple Key Generation (3 keys):
  ghp_v1_eVZTSWRpOGtp_TldIVFhBa1c2Q0MzMVlTb1hwV1JvWUlOcEZUekJCMUFhbmkxa25I
  ghp_v1_YzRra05WN3Nn_NGJxbGZOZDdmNEF6cVcxRktyWjJIazlEME9zOTRlTDBKeUJBMkVs
  ghp_v1_SnEwVW9zbndo_ZldDbVFBTUJjbmJJdmFLdWJpYjF5QkdMS2RNOWtScENIWUFtaDdL

4. High-Security Configuration
------------------------------
Configuration:
  • Prefix: "secure"
  • Version: "v2"
  • Secret Bytes: 48
  • Prefix Bytes: 8
  • Unique ID Length: 12
  • Delimiter: '-'
  • Plus Replacement: '*'
  • Slash Replacement: '

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📚 Additional Resources

- [ASP.NET Core API Key Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [NIST Recommendations for Key Management](https://csrc.nist.gov/publications/detail/sp/800-57-part-1/rev-5/final)

---

Built with ❤️ for the .NET community. If you find SecureApiKeys useful, please consider giving it a star on GitHub!

Generated Key:
  secure-v2-Y2pwUFpYMVdtUE4-NXFXaWdpQmZxUVhNQkZBQ0RqM3JuMUloamprRGR0ZGFTWVRxYUkwTzd1c3NHaDkzWlQ

Key Structure:
  Prefix:    secure
  Version:   v2
  Unique ID: Y2pwUFpYMVdtUE4
  Secret:    NXFXaWdpQmZxUVhNQkZBQ0RqM3JuMUloamprRGR0ZGFTWVRxYUkwTzd1c3NHaDkzWlQ

Estimated Entropy: ~396.0 bits

Multiple Key Generation (3 keys):
  secure-v2-VjZvdHRITURSQVM-a1lLZkg4MHorUVhGa2RxbWN6YW4qQlJkckVyVGpCQlRVYjNYbVFTNDhxYVhFY2gzSjE
  secure-v2-cHFBdGVIOUV3emw-a1BjMm42RDlHY1JPajVOZm1VS1VmdjJDS1FwdnlrelpJYjZ0T1laQVFCYWh4NHdUTjY
  secure-v2-MHYwb2w2R1JkaFc-ZlNXbjM1NTVlZGNoeldpOVl3dzJXSnN6V0dXN29CZkFLSEpoeVVwOHlQUlhHcnBkbWo

5. Dependency Injection Configuration
-------------------------------------
Generated Key:
  di:v3:123456:hOSlmpgplTpyGK2AWgQNHA

Key Structure:
  Prefix:    di
  Version:   v3
  Unique ID: 123456
  Secret:    hOSlmpgplTpyGK2AWgQNHA

Estimated Entropy: ~137.0 bits

Multiple Key Generation (3 keys):
  di:v3:123456:vjGFQIkFm7K9Zu71RQw8SA
  di:v3:123456:RLPdwJKTXtcw7kkMScgRJg
  di:v3:123456:MV9N0vY6vl6X0L6VK8NWDQ

6. Validation and Edge Cases
----------------------------
Valid Key: sk_v1_123456_VkY2lvc3BnRXJmOVpVQ2QyNnp4THpJSENwWEllVU1KNkFxZWQzMWZKQQ
Validates as: ✓ VALID

Invalid Key Tests:
  Null key validates as: ✗ INVALID (Expected: INVALID)
  Empty key validates as: ✗ INVALID (Expected: INVALID)
  Wrong prefix key: xx_v1_123456_VkY2lvc3BnRXJmOVpVQ2QyNnp4THpJSENwWEllVU1KNkFxZWQzMWZKQQ
  Validates as: ✗ INVALID (Expected: INVALID)
  Wrong version key: sk_v2_123456_VkY2lvc3BnRXJmOVpVQ2QyNnp4THpJSENwWEllVU1KNkFxZWQzMWZKQQ
  Validates as: ✗ INVALID (Expected: INVALID)
  Wrong length key: sk_v1_123456_VkY2lvc3BnRXJmOVpVQ2QyNnp4THpJSENwWEllVU1KNkFxZWQzMWZKQQextra
  Validates as: ✗ INVALID (Expected: INVALID)

Invalid Options Tests:
  ✓ SUCCESS: Caught expected exception: SecretBytes must be at least 16 (128 bits) for adequate security
  ✓ SUCCESS: Caught expected exception: '+' cannot be used as a delimiter as it causes issues with string splitting

7. Security Utilities
---------------------
Secure Comparison Test:
Original Key:  sk_v1_123456_VkY2lvc3BnRXJmOVpVQ2QyNnp4THpJSENwWEllVU1KNkFxZWQzMWZKQQ
Same Key:      sk_v1_123456_VkY2lvc3BnRXJmOVpVQ2QyNnp4THpJSENwWEllVU1KNkFxZWQzMWZKQQ
Different Key: sk_v1_123456_ZXJpUDhSbkZwbUcwQUNJV3EwM0hlYW00V2djYXFTclpHemM4YXVkb2Q
Same keys comparison result: ✓ MATCH (Expected: MATCH)
Different keys comparison result: ✗ NO MATCH (Expected: NO MATCH)

API Key Hashing Test:
Original Key:        sk_v1_123456_VkY2lvc3BnRXJmOVpVQ2QyNnp4THpJSENwWEllVU1KNkFxZWQzMWZKQQ
Hashed Key (Base64): j0BCsdg9nS8Ygdj4ityKP7XPwUBJJRuFkoRKClMoImA=
Hashed Again:        j0BCsdg9nS8Ygdj4ityKP7XPwUBJJRuFkoRKClMoImA=
Hash consistency:    ✓ CONSISTENT (Expected: CONSISTENT)
Different Key Hash:  ezT45/sYjh9BZP/Pu4jGaXpch9KXvULO7jBL13/Z1VQ=
Hash uniqueness:     ✓ UNIQUE (Expected: UNIQUE)

All tests completed successfully!
```

### Industry-Standard Key Format Examples

#### Stripe-like API Keys

The sample demonstrates how to configure the library to produce API keys that follow Stripe's format pattern:

```csharp
// Stripe-like API Key Configuration
var stripeOptions = new ApiKeyOptions
{
    Prefix = "sk_test",      // Stripe uses "sk_test" and "sk_live"
    Version = "v1",
    SecretBytes = 24,        // Lower entropy similar to Stripe
    UniqueIdLength = 8,      // Longer unique ID
    Delimiter = '_'          // Same delimiter as Stripe
};

var generator = new SecureApiKeyGenerator(stripeOptions);
var stripeStyleKey = generator.GenerateApiKey();
// Result: sk_test_v1_TkVzMUxSQW9_YVNyU2g0bWdxWU5ZNWVLbEtkbVVFcw
```

Stripe uses API keys in formats like `sk_test_51NrABC...` and `sk_live_51NrABC...` to distinguish between test and live environments. The SecureApiKeys library allows you to create the same pattern by customizing the prefix and other options.

#### GitHub Personal Access Token-like Keys

GitHub uses a different pattern for their Personal Access Tokens (PATs), which the library can also emulate:

```csharp
// GitHub PAT-like Token Configuration
var githubOptions = new ApiKeyOptions
{
    Prefix = "ghp",          // GitHub uses "ghp_" prefix for PATs
    Version = "v1",
    SecretBytes = 36,        // Higher entropy similar to GitHub
    UniqueIdLength = 10,     // Longer unique ID
    Delimiter = '_',
    PlusReplacement = 'A',   // Custom replacements for Base64 special chars
    SlashReplacement = 'B'
};

var generator = new SecureApiKeyGenerator(githubOptions);
var githubStyleKey = generator.GenerateApiKey();
// Result: ghp_v1_dXJZaGVUQnYx_1pBbjFXeGU0dm5TMTNwUGdoVnRiZ1JKcGFVUmR0VjZBOUxJaDE
```

GitHub PATs typically look like `ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx` with a specific structure. By adjusting the options, you can create tokens that match this format while maintaining your own versioning and security requirements.

### Key Structure Visualization

The sample includes a helper to visualize the structure of generated keys:

```csharp
static void VisualizeKeyStructure(string apiKey)
{
    // First, determine the delimiter
    char delimiter = '_'; // Default
    foreach (var possibleDelimiter in new[] { '_', '-', '.', ':', '^', '*' })
    {
        if (apiKey.Contains(possibleDelimiter))
        {
            delimiter = possibleDelimiter;
            break;
        }
    }
    
    var parts = apiKey.Split(delimiter);
    if (parts.Length == 4)
    {
        Console.WriteLine("\nKey Structure:");
        Console.WriteLine($"  Prefix:    {parts[0]}");
        Console.WriteLine($"  Version:   {parts[1]}");
        Console.WriteLine($"  Unique ID: {parts[2]}");
        Console.WriteLine($"  Secret:    {parts[3]}");
        
        // Calculate entropy
        double prefixEntropy = parts[2].Length * Math.Log(64) / Math.Log(2);
        double secretEntropy = parts[3].Length * Math.Log(64) / Math.Log(2);
        double totalEntropy = prefixEntropy + secretEntropy;
        
        Console.WriteLine($"\nEstimated Entropy: ~{totalEntropy:F1} bits");
    }
}
```

### Security Utilities Demo

The sample also demonstrates security utilities such as secure comparison and hashing:

```csharp
// API Key Hashing Test
var keyToHash = generator.GenerateApiKey();
var hashedKey = SecureApiKeyGenerator.HashApiKey(keyToHash);

Console.WriteLine($"Original Key:        {keyToHash}");
Console.WriteLine($"Hashed Key (Base64): {hashedKey}");

// Demonstrate hashing the same key always produces the same result
var hashedKeyAgain = SecureApiKeyGenerator.HashApiKey(keyToHash);
Console.WriteLine($"Hash consistency:    {(hashedKey == hashedKeyAgain ? "✓ CONSISTENT" : "✗ INCONSISTENT")}");
```

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📚 Additional Resources

- [ASP.NET Core API Key Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [NIST Recommendations for Key Management](https://csrc.nist.gov/publications/detail/sp/800-57-part-1/rev-5/final)

---

Built with ❤️ for the .NET community. If you find SecureApiKeys useful, please consider giving it a star on GitHub!