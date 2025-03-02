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

The library includes a complete sample console application that demonstrates all the key features and use cases. Here's what the sample application demonstrates:

### Running the Sample

```bash
# Navigate to the sample directory
cd sample

# Run the sample
dotnet run
```

### What it Demonstrates

The sample showcases several key configurations and use cases:

1. Default API Key Configuration
2. Stripe-like API Key Configuration
3. GitHub PAT-like Token Configuration
4. High-Security Configuration
5. Dependency Injection Configuration
6. Validation and Edge Cases
7. Security Utilities (hashing and secure comparison)

### Sample Output

When you run the sample, you'll see demonstrations of each configuration with generated keys, their structure, and estimated entropy:

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
```

### Code Example from the Sample

Here's an example of key structure visualization from the sample:

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

### Custom API Key Configurations

The sample demonstrates various configurations, including this high-security example:

```csharp
// High-Security Configuration
var secureOptions = new ApiKeyOptions
{
    Prefix = "secure",
    Version = "v2",
    SecretBytes = 48,  // More entropy
    PrefixBytes = 8,
    UniqueIdLength = 12,
    PlusReplacement = '*',
    SlashReplacement = '$',
    Delimiter = '-'
};

var generator = new SecureApiKeyGenerator(secureOptions);
var secureKey = generator.GenerateApiKey();
// Result: secure-v2-a1b2c3d4e5f6-HjKl*nOpQr$tUvWxYz...
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