﻿#region Imports
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

#endregion

namespace Teleopti.Ccc.Domain.Security
{
    /// <summary>
    /// Contains methods for symmetric encrypting and decrypting.
    /// </summary>
    /// <remarks>Current implementation using Rijndael taken from 
    /// http://msdn.microsoft.com/en-us/library/system.security.cryptography.rijndaelmanaged.aspx</remarks>
    public static class Encryption
    {
        /// <summary>
        /// Encrypts the string to bytes using Rijndael AES.
        /// </summary>
        /// <param name="plaintext">The plain text.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The initialization vector.</param>
        /// <returns>encrypted string</returns>
        /// <remarks>Taken from http://msdn.microsoft.com/en-us/library/system.security.cryptography.rijndaelmanaged.aspx</remarks>
        public static byte[] EncryptStringToBytes(string plaintext, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("key");

            // Declare the streams used
            // to encrypt to an in memory
            // array of bytes.
            MemoryStream msEncrypt = null;
            CryptoStream csEncrypt = null;
            StreamWriter swEncrypt = null;

            // Declare the RijndaelManaged object
            // used to encrypt the data.
            RijndaelManaged aesAlg = null;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged {Key = key, IV = iv};
               

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                msEncrypt = new MemoryStream();
                csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                swEncrypt = new StreamWriter(csEncrypt);

                //Write all data to the stream.
                swEncrypt.Write(plaintext);
            }
            finally
            {
                // Clean things up.

                // Close the streams.
                if (swEncrypt != null)
                    swEncrypt.Close();
                if (csEncrypt != null)
                    csEncrypt.Close();
                if (msEncrypt != null)
                    msEncrypt.Close();

                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return msEncrypt.ToArray();
        }

        /// <summary>
        /// Decrypts the string from bytes using Rijndael AES.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The initialization vector.</param>
        /// <returns>decrypted string</returns>
        public static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("key");

            // TDeclare the streams used
            // to decrypt to an in memory
            // array of bytes.
            MemoryStream msDecrypt = null;
            CryptoStream csDecrypt = null;
            StreamReader srDecrypt = null;

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged {Key = key, IV = iv};

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                msDecrypt = new MemoryStream(cipherText);
                csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                srDecrypt = new StreamReader(csDecrypt);

                // Read the decrypted bytes from the decrypting stream
                // and place them in a string.
                plaintext = srDecrypt.ReadToEnd();
            }
            finally
            {
                // Clean things up.

                // Close the streams.
                if (srDecrypt != null)
                    srDecrypt.Close();
                if (csDecrypt != null)
                    csDecrypt.Close();
                if (msDecrypt != null)
                    msDecrypt.Close();
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        /// <summary>
        /// Encrypts the string to a base 64 stringusing Rijndael AES.
        /// </summary>
        /// <param name="plaintext">The plaintext.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        /// <returns>the ciphertext in base64 format</returns>
        public static string EncryptStringToBase64(string plaintext, byte[] key, byte[] iv)
        {
            return Convert.ToBase64String(EncryptStringToBytes(plaintext, key, iv));
        }

        /// <summary>
        /// Decrypts the string from a base 64 formatted string using Rijndael AES.
        /// </summary>
        /// <param name="cipherTextInBase64">The cipher text in base 64 format.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The initialization vector.</param>
        /// <returns>decrypted string</returns>
        public static string DecryptStringFromBase64(string cipherTextInBase64, byte[] key, byte[] iv)
        {
            return DecryptStringFromBytes(Convert.FromBase64String(cipherTextInBase64), key, iv);
        }

        public static IDictionary<TKey, string> DecryptDictionary<TKey>(this IDictionary<TKey,string> encryptedDictionary, byte[] key, byte[] iv)
        {
	        var ret = new Dictionary<TKey, string>(encryptedDictionary.Count);
            foreach (var encryptedKey in encryptedDictionary.Keys.ToList())
            {
                ret[encryptedKey] = DecryptStringFromBase64(encryptedDictionary[encryptedKey], key, iv);
            }
	        return ret;
        }
    }
}