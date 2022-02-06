using Microsoft.Extensions.Options;
using om.shared.api.common.Interfaces;
using om.shared.security.models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace om.shared.api.common.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly string key;

        public CryptoService(IOptions<AuthSettings> options)
        {
            if (options != null && options.Value != null)
            {
                this.key = options.Value.CipherKey;
            }
        }
        private byte[] Key
        {
            get
            {
                return Encoding.UTF8.GetBytes(this.key); ;
            }
        }
        private byte[] Iv
        {
            get
            {
                return Encoding.UTF8.GetBytes(this.key); ;
            }
        }
        public string Encrypt(string plainText)
        {
            using (Aes myAes = Aes.Create())
            {
                // Encrypt the string to an array of bytes.
                byte[] encrypted = EncryptStringToBytes_Aes(plainText, this.Key, this.Iv);

                string cipherText = Convert.ToBase64String(encrypted);
                return cipherText;
            }
        }

        public string Decrypt(string cipherText)
        {
            using (Aes myAes = Aes.Create())
            {
                byte[] encrypted = Convert.FromBase64String(cipherText);
                // Decrypt the bytes to a string.
                string plainText = DecryptStringFromBytes_Aes(encrypted, this.Key, this.Iv);
                return plainText;
            }
        }

        private byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create a RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            using (var rijAlg = new RijndaelManaged())
            {
                //Settings
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;
                rijAlg.Key = key;
                rijAlg.IV = iv;
                // Create a decrytor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
