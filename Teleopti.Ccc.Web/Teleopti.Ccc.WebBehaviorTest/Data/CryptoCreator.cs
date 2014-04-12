using System;
using System.Security.Cryptography;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
    public class CryptoCreator
    {
        public static string GetCryptoBytes(int length)
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[length];
                provider.GetBytes(bytes);

                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }

        public static class MachineKeyCreator
        {
	        private static readonly string Config;
	        const int DecryptionKeyLength = 24;
            const int ValidationKeyLength = 64;
            const string DecryptionType = "AES";
            const string ValidationType = "SHA1";

	        static MachineKeyCreator()
	        {
		        Config = string.Format(
			        "<machineKey validationKey=\"{0}\" decryptionKey=\"{1}\" decryption=\"{2}\" validation=\"{3}\" />",
			        GetCryptoBytes(ValidationKeyLength), GetCryptoBytes(DecryptionKeyLength), DecryptionType,
			        ValidationType);
	        }

	        public static string GetConfig()
            {
                return Config;
            }
        }
    }
}