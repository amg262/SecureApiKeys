using Microsoft.Extensions.DependencyInjection;
using SecureApiKeys;
using System;
using System.Linq;
using System.Text;

namespace Sample;

class Program
{
    private static void Main(string[] args)
    {
        // Set console encoding to ensure all characters display correctly
        Console.OutputEncoding = Encoding.UTF8;
            
        Console.WriteLine("SecureApiKeys Sample Application");
        Console.WriteLine("================================\n");

        try
        {
            // Setup a service collection for dependency injection
            var services = new ServiceCollection();
                
            // Demo 1: Default configuration
            Console.WriteLine("1. Default API Key Configuration");
            Console.WriteLine("----------------------------------");
            var defaultGenerator = new SecureApiKeyGenerator();
            var defaultOptions = new ApiKeyOptions(); // For display purposes
            DisplayOptions(defaultOptions);
            TestGenerator(defaultGenerator);
                
            // Demo 2: Stripe-like keys
            Console.WriteLine("\n2. Stripe-like API Key Configuration");
            Console.WriteLine("------------------------------------");
            var stripeOptions = new ApiKeyOptions
            {
                Prefix = "sk_test",
                Version = "v1",
                SecretBytes = 24,
                UniqueIdLength = 8,
                Delimiter = '_'
            };
            DisplayOptions(stripeOptions);
            TestGenerator(new SecureApiKeyGenerator(stripeOptions));
                
            // Demo 3: GitHub PAT-like tokens
            Console.WriteLine("\n3. GitHub PAT-like Token Configuration");
            Console.WriteLine("---------------------------------------");
            var githubOptions = new ApiKeyOptions
            {
                Prefix = "ghp",
                Version = "v1",
                SecretBytes = 36,
                UniqueIdLength = 10,
                Delimiter = '_',
                PlusReplacement = 'A',
                SlashReplacement = 'B'
            };
            DisplayOptions(githubOptions);
            TestGenerator(new SecureApiKeyGenerator(githubOptions));
                
            // Demo 4: High-security configuration
            Console.WriteLine("\n4. High-Security Configuration");
            Console.WriteLine("------------------------------");
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
            DisplayOptions(secureOptions);
            TestGenerator(new SecureApiKeyGenerator(secureOptions));
                
            // Demo 5: Using with DI container
            Console.WriteLine("\n5. Dependency Injection Configuration");
            Console.WriteLine("-------------------------------------");
            services.AddSecureApiKeys(options => 
            {
                options.Prefix = "di";
                options.Version = "v3";
                options.Delimiter = ':';
                options.SecretBytes = 16;
            });
                
            var serviceProvider = services.BuildServiceProvider();
            var diGenerator = serviceProvider.GetRequiredService<SecureApiKeyGenerator>();
            TestGenerator(diGenerator);
                
            // Demo 6: Edge cases and validation
            Console.WriteLine("\n6. Validation and Edge Cases");
            Console.WriteLine("----------------------------");
            ValidationAndEdgeCases();
                
            // Demo 7: Security utilities
            Console.WriteLine("\n7. Security Utilities");
            Console.WriteLine("---------------------");
            SecurityUtilities();
                
            Console.WriteLine("\nAll tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
            
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
        
    static void DisplayOptions(ApiKeyOptions options)
    {
        Console.WriteLine("Configuration:");
        Console.WriteLine($"  • Prefix: \"{options.Prefix}\"");
        Console.WriteLine($"  • Version: \"{options.Version}\"");
        Console.WriteLine($"  • Secret Bytes: {options.SecretBytes}");
        Console.WriteLine($"  • Prefix Bytes: {options.PrefixBytes}");
        Console.WriteLine($"  • Unique ID Length: {options.UniqueIdLength}");
        Console.WriteLine($"  • Delimiter: '{options.Delimiter}'");
        Console.WriteLine($"  • Plus Replacement: '{options.PlusReplacement}'");
        Console.WriteLine($"  • Slash Replacement: '{options.SlashReplacement}'");
    }
        
    static void TestGenerator(SecureApiKeyGenerator generator)
    {
        // Generate single key
        var singleKey = generator.GenerateApiKey();
        Console.WriteLine("\nGenerated Key:");
        Console.WriteLine($"  {singleKey}");
            
        // Show key structure
        VisualizeKeyStructure(singleKey);
            
        // Generate multiple keys
        Console.WriteLine("\nMultiple Key Generation (3 keys):");
        var multipleKeys = generator.GenerateMultipleApiKeys(3);
        foreach (var key in multipleKeys)
        {
            Console.WriteLine($"  {key}");
        }
    }
        
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
        
    static void ValidationAndEdgeCases()
    {
        var generator = new SecureApiKeyGenerator();
            
        // Generate and validate a key
        var validKey = generator.GenerateApiKey();
        Console.WriteLine($"Valid Key: {validKey}");
        Console.WriteLine($"Validates as: {(generator.ValidateKeyFormat(validKey) ? "✓ VALID" : "✗ INVALID")}");
            
        // Test invalid keys
        Console.WriteLine("\nInvalid Key Tests:");
            
        // Test 1: null or empty
        Console.WriteLine($"  Null key validates as: {(generator.ValidateKeyFormat(null) ? "✓ VALID" : "✗ INVALID")} (Expected: INVALID)");
        Console.WriteLine($"  Empty key validates as: {(generator.ValidateKeyFormat("") ? "✓ VALID" : "✗ INVALID")} (Expected: INVALID)");
            
        // Test 2: Wrong prefix
        var wrongPrefixKey = validKey.Replace("sk_", "xx_");
        Console.WriteLine($"  Wrong prefix key: {wrongPrefixKey}");
        Console.WriteLine($"  Validates as: {(generator.ValidateKeyFormat(wrongPrefixKey) ? "✓ VALID" : "✗ INVALID")} (Expected: INVALID)");
            
        // Test 3: Wrong version
        var wrongVersionKey = validKey.Replace("_v1_", "_v2_");
        Console.WriteLine($"  Wrong version key: {wrongVersionKey}");
        Console.WriteLine($"  Validates as: {(generator.ValidateKeyFormat(wrongVersionKey) ? "✓ VALID" : "✗ INVALID")} (Expected: INVALID)");
            
        // Test 4: Wrong length
        var wrongLengthKey = validKey + "extra";
        Console.WriteLine($"  Wrong length key: {wrongLengthKey}");
        Console.WriteLine($"  Validates as: {(generator.ValidateKeyFormat(wrongLengthKey) ? "✓ VALID" : "✗ INVALID")} (Expected: INVALID)");
            
        // Test invalid options
        Console.WriteLine("\nInvalid Options Tests:");
        try
        {
            var invalidOptions = new ApiKeyOptions
            {
                SecretBytes = 8 // Too low for security
            };
            var invalidGenerator = new SecureApiKeyGenerator(invalidOptions);
            Console.WriteLine("  ✗ ERROR: Should have thrown exception for low SecretBytes");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  ✓ SUCCESS: Caught expected exception: {ex.Message}");
        }
            
        try
        {
            var invalidOptions = new ApiKeyOptions
            {
                Delimiter = '+' // Not allowed
            };
            var invalidGenerator = new SecureApiKeyGenerator(invalidOptions);
            Console.WriteLine("  ✗ ERROR: Should have thrown exception for invalid delimiter");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  ✓ SUCCESS: Caught expected exception: {ex.Message}");
        }
    }
        
    static void SecurityUtilities()
    {
        var generator = new SecureApiKeyGenerator();
            
        // Secure comparison
        Console.WriteLine("Secure Comparison Test:");
        var origKey = generator.GenerateApiKey();
        var sameKey = origKey;
        var differentKey = generator.GenerateApiKey();
            
        Console.WriteLine($"Original Key:  {origKey}");
        Console.WriteLine($"Same Key:      {sameKey}");
        Console.WriteLine($"Different Key: {differentKey}");
            
        bool sameKeyResult = SecureApiKeyGenerator.SecureCompare(origKey, sameKey);
        bool differentKeyResult = SecureApiKeyGenerator.SecureCompare(origKey, differentKey);
            
        Console.WriteLine($"Same keys comparison result: {(sameKeyResult ? "✓ MATCH" : "✗ NO MATCH")} (Expected: MATCH)");
        Console.WriteLine($"Different keys comparison result: {(differentKeyResult ? "✓ MATCH" : "✗ NO MATCH")} (Expected: NO MATCH)");
            
        // API Key Hashing
        Console.WriteLine("\nAPI Key Hashing Test:");
        var keyToHash = generator.GenerateApiKey();
        var hashedKey = SecureApiKeyGenerator.HashApiKey(keyToHash);
            
        Console.WriteLine($"Original Key:        {keyToHash}");
        Console.WriteLine($"Hashed Key (Base64): {hashedKey}");
            
        // Demonstrate hashing the same key always produces the same result
        var hashedKeyAgain = SecureApiKeyGenerator.HashApiKey(keyToHash);
        Console.WriteLine($"Hashed Again:        {hashedKeyAgain}");
        Console.WriteLine($"Hash consistency:    {(hashedKey == hashedKeyAgain ? "✓ CONSISTENT" : "✗ INCONSISTENT")} (Expected: CONSISTENT)");
            
        // Demonstrate hashing different keys produces different results
        var differentKeyHash = SecureApiKeyGenerator.HashApiKey(differentKey);
        Console.WriteLine($"Different Key Hash:  {differentKeyHash}");
        Console.WriteLine($"Hash uniqueness:     {(hashedKey != differentKeyHash ? "✓ UNIQUE" : "✗ COLLISION")} (Expected: UNIQUE)");
    }
}