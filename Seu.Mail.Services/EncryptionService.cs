using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services;

/// <summary>
/// Provides encryption and decryption services for strings and byte arrays, as well as password hashing and verification.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EncryptionService> _logger;
    private readonly byte[] _key;
    private readonly byte[] _iv;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptionService"/> class.
    /// </summary>
    /// <param name="configuration">Application configuration for retrieving encryption keys.</param>
    /// <param name="logger">Logger for encryption events and errors.</param>
    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Get encryption key from configuration or generate a new one
        var keyString = _configuration["Security:EncryptionKey"];
        if (string.IsNullOrEmpty(keyString))
        {
            keyString = GenerateSecureKey();
            _logger.LogWarning("No encryption key found in configuration. Generated a new key. Please save this key: {Key}", keyString);
        }

        _key = Convert.FromBase64String(keyString);

        // Generate or get IV from configuration
        var ivString = _configuration["Security:EncryptionIV"];
        if (string.IsNullOrEmpty(ivString))
        {
            using var aes = Aes.Create();
            _iv = aes.IV;
            _logger.LogWarning("No encryption IV found in configuration. Generated a new IV: {IV}", Convert.ToBase64String(_iv));
        }
        else
        {
            _iv = Convert.FromBase64String(ivString);
        }
    }

    /// <summary>
    /// Encrypts the specified plain text string using AES encryption.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted string in Base64 format.</returns>
    public string Encrypt(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV(); // Generate random IV for each encryption
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText ?? string.Empty);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Prepend IV to encrypted data
            var result = new byte[aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting string");
            throw new SecurityException("Failed to encrypt data", ex);
        }
    }

    /// <summary>
    /// Decrypts the specified cipher text string using AES decryption.
    /// </summary>
    /// <param name="cipherText">The encrypted string in Base64 format.</param>
    /// <returns>The decrypted plain text string.</returns>
    public string Decrypt(string? cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        // First try to handle legacy unencrypted data
        if (IsLegacyPlaintextData(cipherText))
        {
            _logger.LogWarning("Found legacy plaintext password data, will re-encrypt on next save");
            return cipherText ?? string.Empty; // Return as-is for now, will be re-encrypted when saved
        }

        try
        {
            var cipherBytes = Convert.FromBase64String(cipherText ?? string.Empty);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV from the beginning of the cipher text
            var iv = new byte[aes.BlockSize / 8];
            var encryptedData = new byte[cipherBytes.Length - iv.Length];

            Buffer.BlockCopy(cipherBytes, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(cipherBytes, iv.Length, encryptedData, 0, encryptedData.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning("Invalid Base64 format in encrypted data, treating as legacy plaintext: {Error}", ex.Message);
            return cipherText ?? string.Empty; // Treat as legacy plaintext
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting string, attempting legacy fallback");
            // Try one more fallback - return as plaintext if it looks like a reasonable password
            if (IsReasonablePassword(cipherText ?? string.Empty))
            {
                _logger.LogWarning("Treating failed decryption as legacy plaintext password");
                return cipherText ?? string.Empty;
            }
            throw new SecurityException("Failed to decrypt data", ex);
        }
    }

    /// <summary>
    /// Encrypts the specified byte array using AES encryption.
    /// </summary>
    /// <param name="data">The byte array to encrypt.</param>
    /// <returns>The encrypted byte array with IV prepended.</returns>
    public byte[] Encrypt(byte[] data)
    {
        if (data == null || data.Length == 0)
            return Array.Empty<byte>();

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var encryptedBytes = encryptor.TransformFinalBlock(data, 0, data.Length);

            // Prepend IV to encrypted data
            var result = new byte[aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting byte array");
            throw new SecurityException("Failed to encrypt data", ex);
        }
    }

    /// <summary>
    /// Decrypts the specified byte array using AES decryption.
    /// </summary>
    /// <param name="data">The encrypted byte array with IV prepended.</param>
    /// <returns>The decrypted byte array.</returns>
    public byte[] Decrypt(byte[] data)
    {
        if (data == null || data.Length == 0)
            return Array.Empty<byte>();

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV from the beginning of the data
            var iv = new byte[aes.BlockSize / 8];
            var encryptedData = new byte[data.Length - iv.Length];

            Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(data, iv.Length, encryptedData, 0, encryptedData.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting byte array");
            throw new SecurityException("Failed to decrypt data", ex);
        }
    }

    /// <summary>
    /// Encrypts a password string.
    /// </summary>
    /// <param name="password">The password to encrypt.</param>
    /// <returns>The encrypted password string.</returns>
    public string EncryptPassword(string password)
    {
        return Encrypt(password);
    }

    /// <summary>
    /// Decrypts an encrypted password string.
    /// </summary>
    /// <param name="encryptedPassword">The encrypted password string.</param>
    /// <returns>The decrypted password string.</returns>
    public string DecryptPassword(string encryptedPassword)
    {
        return Decrypt(encryptedPassword);
    }

    /// <summary>
    /// Verifies a password against a hash using PBKDF2 and constant time comparison.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The hash to compare against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            var combinedBytes = Convert.FromBase64String(hash);
            if (combinedBytes.Length != 64)
                return false;

            // Extract salt and hash
            var saltBytes = new byte[32];
            var hashBytes = new byte[32];
            Array.Copy(combinedBytes, 0, saltBytes, 0, 32);
            Array.Copy(combinedBytes, 32, hashBytes, 0, 32);

            // Hash the provided password with the extracted salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(32);

            // Compare the hashes in constant time to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(hashBytes, computedHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            return false;
        }
    }

    /// <summary>
    /// Hashes a password using PBKDF2 with a random salt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password string in Base64 format.</returns>
    public string HashPassword(string password)
    {
        // Handle edge cases by treating them as empty strings
        if (string.IsNullOrEmpty(password))
            password = "";

        // Trim whitespace
        password = password.Trim();

        try
        {
            // Generate a random salt
            var saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            // Hash the password with the salt using PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(32);

            // Combine salt and hash
            var combinedBytes = new byte[64]; // 32 salt + 32 hash
            Array.Copy(saltBytes, 0, combinedBytes, 0, 32);
            Array.Copy(hashBytes, 0, combinedBytes, 32, 32);

            return Convert.ToBase64String(combinedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw new SecurityException("Failed to hash password", ex);
        }
    }

    /// <summary>
    /// Encrypts a plain text string and logs the operation.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted string in Base64 format.</returns>
    public string EncryptString(string? plainText)
    {
        _logger.LogWarning("EncryptString called with input length: {Length}", plainText?.Length ?? 0);
        var result = Encrypt(plainText ?? string.Empty);
        _logger.LogWarning("EncryptString result length: {Length}", result?.Length ?? 0);
        return result ?? string.Empty;
    }

    /// <summary>
    /// Decrypts an encrypted string and logs the operation.
    /// </summary>
    /// <param name="cipherText">The encrypted string in Base64 format.</param>
    /// <returns>The decrypted plain text string.</returns>
    public string DecryptString(string? cipherText)
    {
        _logger.LogWarning("DecryptString called with input length: {Length}", cipherText?.Length ?? 0);
        var result = Decrypt(cipherText ?? string.Empty);
        _logger.LogWarning("DecryptString result length: {Length}", result?.Length ?? 0);
        return result ?? string.Empty;
    }

    /// <summary>
    /// Determines if the given string looks like legacy plaintext data
    /// </summary>
    private bool IsLegacyPlaintextData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return false;

        // If it's not valid Base64 and looks like a reasonable password/text, treat as legacy
        try
        {
            Convert.FromBase64String(data);
            return false; // Valid Base64, probably encrypted
        }
        catch
        {
            return IsReasonablePassword(data);
        }
    }

    /// <summary>
    /// Basic heuristic to determine if a string looks like a reasonable password
    /// </summary>
    private bool IsReasonablePassword(string data)
    {
        if (string.IsNullOrEmpty(data))
            return false;

        // Basic checks for reasonable password characteristics
        return data.Length >= 3 &&
               data.Length <= 128 &&
               !data.Contains('\0') &&
               !data.Contains('\n') &&
               !data.Contains('\r');
    }

    /// <summary>
    /// Generates a secure random encryption key.
    /// </summary>
    /// <returns>A secure key string in Base64 format.</returns>
    public string GenerateSecureKey()
    {
        try
        {
            var keyBytes = new byte[32]; // 256-bit key
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            return Convert.ToBase64String(keyBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating secure key");
            throw new SecurityException("Failed to generate secure key", ex);
        }
    }
}

/// <summary>
/// Represents errors that occur during encryption or security operations.
/// </summary>
public class SecurityException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SecurityException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception reference.</param>
    public SecurityException(string message, Exception innerException) : base(message, innerException) { }
}
