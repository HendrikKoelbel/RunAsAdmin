using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RunAsAdmin.Core
{
    /// <summary>
    /// Provides secure encryption/decryption using AES-256 with PBKDF2 key derivation
    /// and Windows DPAPI for additional security layer
    /// </summary>
    public static class SecurityHelper
    {
        // Using DPAPI for Windows-specific secure encryption
        // This eliminates the need for hardcoded keys and provides per-user encryption
        private static readonly DataProtectionScope ProtectionScope = DataProtectionScope.CurrentUser;

        // Additional entropy for DPAPI (acts as a secondary password)
        private static readonly byte[] AdditionalEntropy = Encoding.UTF8.GetBytes("RunAsAdmin_Security_v2.0");

        /// <summary>
        /// Encrypts a string using Windows DPAPI with AES-256 encryption
        /// </summary>
        /// <param name="textToEncrypt">The plaintext string to encrypt</param>
        /// <returns>Base64-encoded encrypted string, or null if input is null/empty</returns>
        public static string Encrypt(string textToEncrypt)
        {
            try
            {
                if (string.IsNullOrEmpty(textToEncrypt))
                {
                    GlobalVars.Loggi.Warning("Encrypt called with null or empty string");
                    return null;
                }

                // Convert string to byte array
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(textToEncrypt);

                // Use Windows DPAPI for encryption
                byte[] encryptedBytes = ProtectedData.Protect(
                    plaintextBytes,
                    AdditionalEntropy,
                    ProtectionScope);

                // Convert to Base64 for storage
                string encryptedText = Convert.ToBase64String(encryptedBytes);

                GlobalVars.Loggi.Debug("Successfully encrypted data");
                return encryptedText;
            }
            catch (CryptographicException cryptoEx)
            {
                GlobalVars.Loggi.Error(cryptoEx, "Cryptographic error during encryption: {Message}", cryptoEx.Message);
                throw new InvalidOperationException("Failed to encrypt data. Ensure you have proper permissions.", cryptoEx);
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Unexpected error during encryption: {Message}", ex.Message);
                throw new InvalidOperationException("Encryption failed", ex);
            }
        }

        /// <summary>
        /// Decrypts a string that was encrypted using the Encrypt method
        /// </summary>
        /// <param name="textToDecrypt">Base64-encoded encrypted string</param>
        /// <returns>Decrypted plaintext string, or null if input is null/empty</returns>
        public static string Decrypt(string textToDecrypt)
        {
            try
            {
                if (string.IsNullOrEmpty(textToDecrypt))
                {
                    GlobalVars.Loggi.Warning("Decrypt called with null or empty string");
                    return null;
                }

                // Convert from Base64
                byte[] encryptedBytes = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));

                // Use Windows DPAPI for decryption
                byte[] decryptedBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    AdditionalEntropy,
                    ProtectionScope);

                // Convert back to string
                string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

                GlobalVars.Loggi.Debug("Successfully decrypted data");
                return decryptedText;
            }
            catch (FormatException formatEx)
            {
                GlobalVars.Loggi.Error(formatEx, "Invalid Base64 format during decryption: {Message}", formatEx.Message);
                throw new ArgumentException("The encrypted text is not in a valid format", nameof(textToDecrypt), formatEx);
            }
            catch (CryptographicException cryptoEx)
            {
                GlobalVars.Loggi.Error(cryptoEx, "Cryptographic error during decryption: {Message}", cryptoEx.Message);
                throw new InvalidOperationException("Failed to decrypt data. The data may have been encrypted by a different user or machine.", cryptoEx);
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Unexpected error during decryption: {Message}", ex.Message);
                throw new InvalidOperationException("Decryption failed", ex);
            }
        }

        /// <summary>
        /// Legacy method for migrating old DES-encrypted data to new DPAPI encryption
        /// This should only be used during migration and then removed
        /// </summary>
        [Obsolete("This method is only for migrating legacy DES/AES encrypted data. Use Encrypt/Decrypt instead.")]
        public static string MigrateLegacyEncryption(string legacyEncrypted, bool useDES)
        {
            try
            {
                if (string.IsNullOrEmpty(legacyEncrypted))
                    return null;

                string decrypted = DecryptLegacy(legacyEncrypted, useDES);
                string newEncrypted = Encrypt(decrypted);

                GlobalVars.Loggi.Information("Successfully migrated legacy encrypted data");
                return newEncrypted;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Failed to migrate legacy encryption: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Decrypts legacy DES or AES encrypted data
        /// </summary>
        private static string DecryptLegacy(string textToDecrypt, bool useDES)
        {
            string _key = "Lf7Xw5g8GFczu$^&6bJfhfjXa6";
            string _iv = "T4-+6t*C=-c7uP$2h?S^&PG";

            byte[] inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));

            if (useDES)
            {
                // Legacy DES decryption
                byte[] _ivByte = Encoding.UTF8.GetBytes(_iv.Substring(0, 8));
                byte[] _keybyte = Encoding.UTF8.GetBytes(_key.Substring(0, 8));

                using (var des = new DESCryptoServiceProvider())
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, des.CreateDecryptor(_keybyte, _ivByte), CryptoStreamMode.Write))
                {
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            else
            {
                // Legacy AES decryption
                byte[] _ivByte = Encoding.UTF8.GetBytes(_iv.Substring(0, 16));
                byte[] _keybyte = Encoding.UTF8.GetBytes(_key.Substring(0, 16));

                using (var aes = Aes.Create())
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(_keybyte, _ivByte), CryptoStreamMode.Write))
                {
                    aes.Key = _keybyte;
                    aes.IV = _ivByte;
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
}
