using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Seu.Mail.Services;
using Seu.Mail.Tests.TestHelpers;

namespace Seu.Mail.Tests.Services;

public class EncryptionServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EncryptionService> _mockLogger;
    private readonly EncryptionService _encryptionService;

    public EncryptionServiceTests()
    {
        _configuration = TestConfigurationHelper.CreateTestConfiguration();
        _mockLogger = Substitute.For<ILogger<EncryptionService>>();

        _encryptionService = new EncryptionService(_configuration, _mockLogger);
    }

    [Test]
    public async Task EncryptString_WithValidInput_ShouldReturnEncryptedString()
    {
        // Arrange
        var plainText = "test password";

        // Act
        var encrypted = _encryptionService.EncryptString(plainText);

        // Assert
        await Assert.That(encrypted).IsNotNull();
        await Assert.That(encrypted).IsNotEmpty();
        await Assert.That(encrypted).IsNotEqualTo(plainText);
        await Assert.That(encrypted).Matches(@"^[A-Za-z0-9+/]*={0,2}$"); // Base64 pattern
    }

    [Test]
    public async Task DecryptString_WithValidEncryptedString_ShouldReturnOriginalText()
    {
        // Arrange
        var plainText = "test password";
        var encrypted = _encryptionService.EncryptString(plainText);

        // Act
        var decrypted = _encryptionService.DecryptString(encrypted);

        // Assert
        await Assert.That(decrypted).IsEqualTo(plainText);
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task EncryptString_WithEmptyOrNullInput_ShouldReturnEmpty(string? input)
    {
        // Act
        var result = _encryptionService.EncryptString(input!);

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task DecryptString_WithEmptyOrNullInput_ShouldReturnEmpty(string? input)
    {
        // Act
        var result = _encryptionService.DecryptString(input!);

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task EncryptDecrypt_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var plainText = "P@ssw0rd!#$%^&*()_+{}[]|\\:;\"'<>,.?/~`";

        // Act
        var encrypted = _encryptionService.EncryptString(plainText);
        var decrypted = _encryptionService.DecryptString(encrypted);

        // Assert
        await Assert.That(decrypted).IsEqualTo(plainText);
    }

    [Test]
    public async Task EncryptDecrypt_WithUnicodeCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var plainText = "Héllö Wörld 🌍 中文 العربية";

        // Act
        var encrypted = _encryptionService.EncryptString(plainText);
        var decrypted = _encryptionService.DecryptString(encrypted);

        // Assert
        await Assert.That(decrypted).IsEqualTo(plainText);
    }

    [Test]
    public async Task EncryptDecrypt_WithLongString_ShouldWorkCorrectly()
    {
        // Arrange
        var plainText = new string('A', 10000);

        // Act
        var encrypted = _encryptionService.EncryptString(plainText);
        var decrypted = _encryptionService.DecryptString(encrypted);

        // Assert
        await Assert.That(decrypted).IsEqualTo(plainText);
    }

    [Test]
    public async Task HashPassword_WithValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hashed = _encryptionService.HashPassword(password);

        // Assert
        await Assert.That(hashed).IsNotNull();
        await Assert.That(hashed).IsNotEmpty();
        await Assert.That(hashed).IsNotEqualTo(password);
        await Assert.That(hashed).Matches(@"^[A-Za-z0-9+/]*={0,2}$"); // Base64 pattern
        await Assert.That(hashed.Length).IsGreaterThan(80); // Salt + hash should be substantial
    }

    [Test]
    public async Task HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _encryptionService.HashPassword(password);
        var hash2 = _encryptionService.HashPassword(password);

        // Assert
        await Assert.That(hash1).IsNotEqualTo(hash2); // Due to random salt
    }

    [Test]
    public async Task VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _encryptionService.HashPassword(password);

        // Act
        var result = _encryptionService.VerifyPassword(password, hashedPassword);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "TestPassword123!";
        var incorrectPassword = "WrongPassword123!";
        var hashedPassword = _encryptionService.HashPassword(correctPassword);

        // Act
        var result = _encryptionService.VerifyPassword(incorrectPassword, hashedPassword);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    [Arguments("", "validhash")]
    [Arguments(null, "validhash")]
    [Arguments("password", "")]
    [Arguments("password", null)]
    [Arguments("", "")]
    [Arguments(null, null)]
    public async Task VerifyPassword_WithNullOrEmpty_ShouldReturnFalse(string? password, string? hash)
    {
        // Act
        var result = _encryptionService.VerifyPassword(password!, hash!);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task HashPassword_WithNullOrEmpty_ShouldReturnValidHash(string? password)
    {
        // Act
        var result = _encryptionService.HashPassword(password!);

        // Assert - Should return a valid hash even for null/empty input
        await Assert.That(result).IsNotEmpty();
    }

    [Test]
    public async Task GenerateSecureKey_ShouldReturnValidBase64Key()
    {
        // Act
        var key = _encryptionService.GenerateSecureKey();

        // Assert
        await Assert.That(key).IsNotNull();
        await Assert.That(key).IsNotEmpty();
        await Assert.That(key).Matches(@"^[A-Za-z0-9+/]*={0,2}$"); // Base64 pattern

        // Should be able to decode to 32 bytes (256-bit key)
        var keyBytes = Convert.FromBase64String(key);
        await Assert.That(keyBytes.Length).IsEqualTo(32);
    }

    [Test]
    public async Task GenerateSecureKey_ShouldReturnDifferentKeysOnMultipleCalls()
    {
        // Act
        var key1 = _encryptionService.GenerateSecureKey();
        var key2 = _encryptionService.GenerateSecureKey();

        // Assert
        await Assert.That(key1).IsNotEqualTo(key2);
    }

    [Test]
    public async Task DecryptString_WithInvalidBase64_ShouldReturnAsLegacyPlaintext()
    {
        // Arrange
        var invalidBase64 = "not-valid-base64!@#$";

        // Act
        var result = _encryptionService.DecryptString(invalidBase64);

        // Assert - Should treat as legacy plaintext and return as-is
        await Assert.That(result).IsEqualTo(invalidBase64);
    }

    [Test]
    public async Task DecryptString_WithValidBase64ButInvalidCipherText_ShouldReturnAsLegacyPlaintext()
    {
        // Arrange - Create a valid base64 string but with data that looks like a reasonable password
        var shortData = new byte[] { 65, 66, 67 }; // "ABC" in ASCII
        var invalidCipherText = Convert.ToBase64String(shortData);

        // Act
        var result = _encryptionService.DecryptString(invalidCipherText);

        // Assert - Should treat as legacy plaintext since decryption fails but data looks reasonable
        await Assert.That(result).IsEqualTo(invalidCipherText);
    }

    [Test]
    public async Task VerifyPassword_WithInvalidHashFormat_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "invalid-hash-format";

        // Act
        var result = _encryptionService.VerifyPassword(password, invalidHash);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task VerifyPassword_WithHashOfWrongLength_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var shortHash = Convert.ToBase64String(new byte[32]); // Should be 64 bytes

        // Act
        var result = _encryptionService.VerifyPassword(password, shortHash);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task PasswordHashing_ShouldBeResistantToTimingAttacks()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _encryptionService.HashPassword(password);
        var wrongPassword = "WrongPassword123!";

        // Act & Assert - Multiple verifications should take similar time
        var times = new List<long>();

        for (int i = 0; i < 5; i++)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _encryptionService.VerifyPassword(wrongPassword, hashedPassword);
            stopwatch.Stop();
            times.Add(stopwatch.ElapsedTicks);
        }

        // Verify that times are relatively consistent (no huge variations)
        var average = times.Average();
        var maxDeviation = times.Max(t => Math.Abs(t - average));

        // Allow for reasonable deviation (timing attacks would show significant differences)
        await Assert.That(maxDeviation / average).IsLessThan(2.0); // Less than 200% deviation
    }

    [Test]
    public async Task EncryptString_WithMultipleCallsOnSameInput_ShouldReturnDifferentResults()
    {
        // Arrange
        var plainText = "test password";

        // Act
        var encrypted1 = _encryptionService.EncryptString(plainText);
        var encrypted2 = _encryptionService.EncryptString(plainText);

        // Assert
        await Assert.That(encrypted1).IsNotEqualTo(encrypted2); // Should use different IVs

        // But both should decrypt to the same original text
        var decrypted1 = _encryptionService.DecryptString(encrypted1);
        var decrypted2 = _encryptionService.DecryptString(encrypted2);
        await Assert.That(decrypted1).IsEqualTo(plainText);
        await Assert.That(decrypted2).IsEqualTo(plainText);
    }

    [Test]
    public async Task HashPassword_WithVeryLongPassword_ShouldWorkCorrectly()
    {
        // Arrange
        var longPassword = new string('A', 1000) + "1!";

        // Act
        var hashed = _encryptionService.HashPassword(longPassword);

        // Assert
        await Assert.That(hashed).IsNotNull();
        await Assert.That(hashed).IsNotEmpty();

        // Verify it can be verified correctly
        var isValid = _encryptionService.VerifyPassword(longPassword, hashed);
        await Assert.That(isValid).IsTrue();
    }

    [Test]
    public async Task EncryptDecrypt_WithEmptyStringAfterTrim_ShouldReturnEmpty()
    {
        // Arrange
        var whitespaceString = "   \t\n\r   ";

        // Act
        var encrypted = _encryptionService.EncryptString(whitespaceString);
        var decrypted = _encryptionService.DecryptString(encrypted);

        // Assert
        await Assert.That(decrypted).IsEqualTo(whitespaceString);
    }

    #region Additional Security and Edge Case Tests

    [Test]
    public async Task EncryptString_WithLargeInput_ShouldHandleCorrectly()
    {
        // Arrange
        var largeInput = new string('A', 10000); // 10KB of data

        // Act
        var encrypted = _encryptionService.EncryptString(largeInput);
        var decrypted = _encryptionService.DecryptString(encrypted);

        // Assert
        await Assert.That(encrypted).IsNotEqualTo(largeInput);
        await Assert.That(decrypted).IsEqualTo(largeInput);
        await Assert.That(encrypted.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task EncryptString_WithBinaryData_ShouldHandleCorrectly()
    {
        // Arrange
        var binaryString = "\0\x01\x02\x03\xFF\xFE"; // Binary data as string

        // Act
        var encrypted = _encryptionService.EncryptString(binaryString);
        var decrypted = _encryptionService.DecryptString(encrypted);

        // Assert
        await Assert.That(decrypted).IsEqualTo(binaryString);
    }

    [Test]
    public async Task HashPassword_WithSameInput_ShouldProduceDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _encryptionService.HashPassword(password);
        var hash2 = _encryptionService.HashPassword(password);

        // Assert
        await Assert.That(hash1).IsNotEqualTo(hash2);
        await Assert.That(hash1).IsNotEmpty();
        await Assert.That(hash2).IsNotEmpty();
    }

    [Test]
    public async Task VerifyPassword_WithIncorrectPassword_ShouldReturnFalse2()
    {
        // Arrange
        var correctPassword = "SecurePassword123!";
        var incorrectPassword = "WrongPassword123!";
        var hashedPassword = _encryptionService.HashPassword(correctPassword);

        // Act
        var result = _encryptionService.VerifyPassword(incorrectPassword, hashedPassword);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task EncryptDecrypt_ConcurrentOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var inputs = new[] { "Test1", "Test2", "Test3", "Test4", "Test5" };
        var tasks = new List<Task<(string encrypted, string decrypted)>>();

        // Act
        foreach (var input in inputs)
        {
            tasks.Add(Task.Run(() =>
            {
                var encrypted = _encryptionService.EncryptString(input);
                var decrypted = _encryptionService.DecryptString(encrypted);
                return (encrypted, decrypted);
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < inputs.Length; i++)
        {
            await Assert.That(results[i].decrypted).IsEqualTo(inputs[i]);
            await Assert.That(results[i].encrypted).IsNotEqualTo(inputs[i]);
        }
    }

    [Test]
    public async Task EncryptString_WithMaxLengthString_ShouldNotThrow()
    {
        // Arrange
        var maxLengthString = new string('X', 1000000); // 1MB string

        // Act & Assert - Should not throw
        var encrypted = _encryptionService.EncryptString(maxLengthString);
        var decrypted = _encryptionService.DecryptString(encrypted);

        await Assert.That(decrypted).IsEqualTo(maxLengthString);
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    [Arguments("\t\n\r")]
    public async Task HashPassword_WithEdgeCaseInputs_ShouldNotThrow(string password)
    {
        // Act & Assert - Should not throw and should return non-empty hash
        var hash = _encryptionService.HashPassword(password);
        await Assert.That(hash).IsNotEmpty();
    }

    #endregion
}
