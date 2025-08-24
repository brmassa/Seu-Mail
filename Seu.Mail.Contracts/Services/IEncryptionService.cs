namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for encryption and decryption operations.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plain text string.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted string.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted string.
    /// </summary>
    /// <param name="cipherText">The encrypted string to decrypt.</param>
    /// <returns>The decrypted plain text.</returns>
    string Decrypt(string cipherText);

    /// <summary>
    /// Encrypts a byte array.
    /// </summary>
    /// <param name="data">The data to encrypt.</param>
    /// <returns>The encrypted byte array.</returns>
    byte[] Encrypt(byte[] data);

    /// <summary>
    /// Decrypts an encrypted byte array.
    /// </summary>
    /// <param name="data">The encrypted data to decrypt.</param>
    /// <returns>The decrypted byte array.</returns>
    byte[] Decrypt(byte[] data);

    /// <summary>
    /// Encrypts a password string.
    /// </summary>
    /// <param name="password">The password to encrypt.</param>
    /// <returns>The encrypted password string.</returns>
    string EncryptPassword(string password);

    /// <summary>
    /// Decrypts an encrypted password string.
    /// </summary>
    /// <param name="encryptedPassword">The encrypted password to decrypt.</param>
    /// <returns>The decrypted password string.</returns>
    string DecryptPassword(string encryptedPassword);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The hash to compare against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Hashes a password string.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password string.</returns>
    string HashPassword(string password);

    // Additional methods expected by services

    /// <summary>
    /// Encrypts a plain text string (alternative method).
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted string.</returns>
    string EncryptString(string plainText);

    /// <summary>
    /// Decrypts an encrypted string (alternative method).
    /// </summary>
    /// <param name="cipherText">The encrypted string to decrypt.</param>
    /// <returns>The decrypted plain text.</returns>
    string DecryptString(string cipherText);

    /// <summary>
    /// Generates a secure encryption key.
    /// </summary>
    /// <returns>A secure key string.</returns>
    string GenerateSecureKey();
}
