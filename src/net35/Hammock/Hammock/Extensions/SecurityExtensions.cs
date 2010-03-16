#if !Smartphone
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
#endif

namespace Hammock.Extensions
{
#if !Smartphone
    internal static class SecurityExtensions
    {
        private static readonly byte[] _entropy = Encoding.Unicode.GetBytes("rosebud");

        public static byte[] Encrypt(this byte[] data)
        {
            if (data.LongLength == 0)
            {
                return data;
            }

            var encrypted = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
            return encrypted;
        }

        public static string Encrypt(this SecureString input)
        {
            if (input == null)
            {
                return null;
            }

            var bytes = Encoding.Unicode.GetBytes(input.Insecure());
            var encrypted = bytes.Encrypt();

            var output = Convert.ToBase64String(encrypted);
            Array.Clear(encrypted, 0, encrypted.Length);

            return output;
        }

        public static byte[] Decrypt(this byte[] data)
        {
            if (data.LongLength == 0)
            {
                return data;
            }

            var decrypted = ProtectedData.Unprotect(data, _entropy, DataProtectionScope.CurrentUser);
            return decrypted;
        }

        public static SecureString Decrypt(this string encryptedData)
        {
            if (encryptedData == null)
            {
                return null;
            }

            try
            {
                var bytes = Convert.FromBase64String(encryptedData);
                var decrypted = bytes.Decrypt();

                var output = Encoding.Unicode.GetString(decrypted).Secure();
                Array.Clear(decrypted, 0, decrypted.Length);

                return output;
            }
            catch
            {
                return new SecureString();
            }
        }

        public static SecureString Secure(this string input)
        {
            if (input == null)
            {
                return null;
            }

            var secure = new SecureString();
            foreach (var c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string Insecure(this SecureString input)
        {
            if (input == null)
            {
                return null;
            }

            string returnValue;
            var ptr = Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
    }
#endif
}