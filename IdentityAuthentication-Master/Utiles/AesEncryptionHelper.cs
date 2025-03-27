using System.Security.Cryptography;
using System.Text;

namespace IdentityAuthentication_Master.Utiles
{
    public static class AesEncryptionHelper
    {
        private static readonly string Key = "khQ+YAXuWDDjyCrFDHP742AJI6DHWZwR+M4kro6bS/o=";
        private static readonly string IV = "NlkKGE196okq2AqztNPdWQ==";
        private static readonly HashSet<char> Base64Chars = new HashSet<char>(
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="
        );

        public static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(Key);
                aes.IV = Convert.FromBase64String(IV);

                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    cs.Write(inputBytes, 0, inputBytes.Length);
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray())
                        .Replace(" ", "").Replace("\n", ""); // 确保无空格/换行
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            // 验证 Base64 格式
            if (string.IsNullOrWhiteSpace(cipherText) ||
                cipherText.Length % 4 != 0 ||
                cipherText.Any(c => !Base64Chars.Contains(c)))
            {
                throw new ArgumentException("Invalid Base64 string.");
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(Key);
                aes.IV = Convert.FromBase64String(IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cs, Encoding.UTF8))
                {
                    return reader.ReadToEnd().TrimEnd('\0');
                }
            }
        }
    }
}
