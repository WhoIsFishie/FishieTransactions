using System.Security.Cryptography;
using System.Text;

namespace FishieTransactions.Helper
{
    public class SecurityManager
    {
        public static byte[] EncryptStringToBytes(string plainText, string password)
        {

            byte[] plainBytes = System.Text.Encoding.Unicode.GetBytes(plainText);

            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.GenerateIV();
            symmetricKey.Key = System.Text.Encoding.Unicode.GetBytes(ComputeSha256Hash(password));

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, symmetricKey.CreateEncryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();

            byte[] encrypted = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            return encrypted;
        }

        public static string DecryptBytesToString(byte[] encrypted, string password)
        {
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Key = System.Text.Encoding.Unicode.GetBytes(ComputeSha256Hash(password));

            MemoryStream memoryStream = new MemoryStream(encrypted);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, symmetricKey.CreateDecryptor(), CryptoStreamMode.Read);

            byte[] plainBytes = new byte[encrypted.Length];
            int decryptedByteCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);

            memoryStream.Close();
            cryptoStream.Close();

            return System.Text.Encoding.Unicode.GetString(plainBytes, 0, decryptedByteCount);
        }

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
