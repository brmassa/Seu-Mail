using Microsoft.Extensions.Configuration;

namespace Seu.Mail.Tests.TestHelpers;

/// <summary>
/// Helper class to create test configuration for services that require configuration
/// </summary>
public static class TestConfigurationHelper
{
    /// <summary>
    /// Creates a test configuration with encryption settings
    /// </summary>
    /// <returns>Test configuration with required encryption keys</returns>
    public static IConfiguration CreateTestConfiguration()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Security:EncryptionKey"] = "5/U96nPGtqXEh9IV/EIQk/iBR+uv/iPyqd56aWLBK6c=", // Base64 encoded 32-byte key
            ["Security:EncryptionIV"] = "jY4r610LHle4RWz/fxd+TQ==" // Base64 encoded 16-byte IV
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    /// <summary>
    /// Creates a minimal test configuration
    /// </summary>
    /// <returns>Empty test configuration</returns>
    public static IConfiguration CreateEmptyConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
    }

    /// <summary>
    /// Creates test configuration with custom settings
    /// </summary>
    /// <param name="configData">Configuration data to include</param>
    /// <returns>Test configuration with specified data</returns>
    public static IConfiguration CreateCustomConfiguration(Dictionary<string, string?> configData)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }
}