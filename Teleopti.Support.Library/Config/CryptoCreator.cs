using System;
using System.Security.Cryptography;

namespace Teleopti.Support.Library.Config
{
    public class CryptoCreator
    {
        public static string GetCryptoBytes(int length)
        {
            string result;
            using (var rNgCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var array = new byte[length];
                rNgCryptoServiceProvider.GetBytes(array);
                result = BitConverter.ToString(array).Replace("-", "");
            }
            return result;
        }
    }
}