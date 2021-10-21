using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.DataStore.Utility
{
    public static class EncryptDecrypt
    {
        static String key = "PENPAL2017Key";
        static String iv = "PENPAL2017IV";
        static byte[] getKeyBytes(String keyBytes)
        {

            byte[] keyBytes1 = new byte[32];
            byte[] parameterKeyBytes = System.Text.Encoding.UTF8.GetBytes(keyBytes);
            //byte[] parameterKeyBytes = Convert.FromBase64String(keyBytes);
            Array.Copy(parameterKeyBytes, 0, keyBytes1, 0, Math.Min(parameterKeyBytes.Length, keyBytes1.Length));

            return keyBytes1;
        }

        static byte[] getIVBytes(string key)
        {

            byte[] keyBytes1 = new byte[16];
            byte[] parameterKeyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            //byte[] parameterKeyBytes = Convert.FromBase64String(key);
            Array.Copy(parameterKeyBytes, 0, keyBytes1, 0, Math.Min(parameterKeyBytes.Length, keyBytes1.Length));

            return keyBytes1;

        }

        public static String getKeyBytes()
        {

            byte[] keyBytes = getKeyBytes(key);
            String ky = Array2String(keyBytes);//System.Text.Encoding.Default.GetString(keyBytes);
            return ky;
        }

        public static String getIvBytes()
        {

            byte[] keyBytes = getIVBytes(iv);
            // String ky = System.Text.Encoding.UTF8.GetString(keyBytes);  //System.Text.Encoding.Default.GetString(keyBytes);

            String ky = Array2String(keyBytes);
            return ky;
        }

        static string Array2String<T>(IEnumerable<T> list)
        {
            return "[" + string.Join(",", list) + "]";
        }

        public static string Encrypt(string PlainText)
        {
            //string EncryptionKey = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["EncryptionKey"]);
            //string EncryptionIv = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["EncryptionIv"]);
            //string tempKey="[44,252,33,111,31,174,221,70,187,137,206,79,238,129,121,122,156,176,84,109,31,234,199,50,231,57,64,14,126,183,183,25]";
            //string tempIv="[225,0,190,169,236,236,113,97,43,102,217,8,113,201,50,45]";

            //string key = string.IsNullOrEmpty(EncryptionKey) ? tempKey : EncryptionKey;
            //string iv = string.IsNullOrEmpty(EncryptionIv) ? tempIv : EncryptionIv;


            string keyforUser = getKeyBytes();
            string ivforUser = getIvBytes();

            byte[] keyBytes = getKeyBytes(key);
            byte[] ivBytes = getIVBytes(iv);
            RijndaelManaged aes = new RijndaelManaged();
            //aes.BlockSize = 128;
            //aes.KeySize = 256;

            /// In Java, Same with below code
            /// Cipher _Cipher = Cipher.getInstance("AES");  // Java Code
            aes.Mode = CipherMode.CBC;
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            ICryptoTransform encrypto = aes.CreateEncryptor();

            //byte[] plainTextByte = ASCIIEncoding.UTF8.GetBytes(PlainText);
            byte[] plainTextByte = Encoding.UTF8.GetBytes(PlainText);
            byte[] CipherText = encrypto.TransformFinalBlock(plainTextByte, 0, plainTextByte.Length);
            return BitConverter.ToString(CipherText).Replace("-", string.Empty); ;
            //return Convert.ToBase64String(CipherText);
            //return BitConverter.ToString(CipherText);
        }
        //static void GetTwoNumbers(out int number1, out int number2)
        //{
        //    number1 = (int)Math.Pow(2, 2);
        //    number2 = (int)Math.Pow(3, 2);
        //}

        public static string Decrypt(string encryptedText)
        {
            // string EncryptionKey = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["EncryptionKey"]);
            //string EncryptionIv = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["EncryptionIv"]);
            // string tempKey = "[44,252,33,111,31,174,221,70,187,137,206,79,238,129,121,122,156,176,84,109,31,234,199,50,231,57,64,14,126,183,183,25]";
            //string tempIv = "[225,0,190,169,236,236,113,97,43,102,217,8,113,201,50,45]";

            // string key = string.IsNullOrEmpty(EncryptionKey) ? tempKey : EncryptionKey;
            //string iv = string.IsNullOrEmpty(EncryptionIv) ? tempIv : EncryptionIv;

            //encryptedText = encryptedText.Insert(2, "-");
            int length = encryptedText.Length;
            byte[] keyBytes = getKeyBytes(key);
            byte[] ivBytes = getIVBytes(iv);

            string encrytedTextNew = "";
            char[] encrytArray = encryptedText.ToCharArray(0, encryptedText.Length);
            for (int i = 0; i < encryptedText.Length; i++)
            {

                if (i != 0)
                {
                    int j = i + 1;
                    if (j % 2 == 0)
                    {

                        encrytedTextNew = encrytedTextNew + encrytArray[i] + "-";
                    }
                    else
                    {

                        encrytedTextNew = encrytedTextNew + encrytArray[i];
                    }

                }
                else if (i == 0)
                {

                    encrytedTextNew = encrytedTextNew + encrytArray[i];
                }
            }


            encrytedTextNew = encrytedTextNew.Remove(encrytedTextNew.Length - 1);

            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.CBC;
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            ICryptoTransform encrypto = aes.CreateDecryptor();

            byte[] plainTextByte = Array.ConvertAll<string, byte>(encrytedTextNew.Split('-'), s => Convert.ToByte(s, 16));
            byte[] CipherText = encrypto.TransformFinalBlock(plainTextByte, 0, plainTextByte.Length);
            return ASCIIEncoding.UTF8.GetString(CipherText);


        }







        

    }
}
