using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DoAn_NT106.Services
{
    /// <summary>
    /// Service mã hóa/giải mã tin nhắn sử dụng AES-256
    /// </summary>
    public class EncryptionService
    {
        private static readonly byte[] KEY = GetFixedBytes("DoAn_NT106_SecretKey", 32);
        private static readonly byte[] IV = GetFixedBytes("DoAn_NT106_IV", 16);

        /// <summary>
        /// Tạo mảng byte có độ dài cố định từ string
        /// </summary>
        private static byte[] GetFixedBytes(string input, int length)
        {
            byte[] result = new byte[length];
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // Copy input vào result, nếu thiếu thì pad 0
            Array.Copy(inputBytes, result, Math.Min(inputBytes.Length, length));

            return result;
        }


        /// <summary>
        /// Mã hóa chuỗi văn bản thành Base64 và thêm newline delimiter
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = KEY;
                    aes.IV = IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                        }
                        // ✅ THÊM NEWLINE DELIMITER để phân tách messages
                        return Convert.ToBase64String(msEncrypt.ToArray()) + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Encryption error: {ex.Message}");
                throw new Exception("Encryption failed", ex);
            }
        }

        /// <summary>
        /// Giải mã chuỗi Base64 thành văn bản gốc
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                // ✅ Loại bỏ newline nếu có
                cipherText = cipherText.Trim('\n', '\r');

                using (Aes aes = Aes.Create())
                {
                    aes.Key = KEY;
                    aes.IV = IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    byte[] buffer = Convert.FromBase64String(cipherText);

                    using (MemoryStream msDecrypt = new MemoryStream(buffer))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Decryption error: {ex.Message}");
                throw new Exception("Decryption failed", ex);
            }
        }

        /// <summary>
        /// Kiểm tra xem chuỗi có phải là Base64 hợp lệ không
        /// </summary>
        public static bool IsBase64String(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return false;

            try
            {
                Convert.FromBase64String(base64);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}