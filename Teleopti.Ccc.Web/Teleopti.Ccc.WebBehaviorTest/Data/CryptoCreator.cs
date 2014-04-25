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

	        public static string StaticMachineKeyForBehaviorTest()
	        {
		        return
			        "<machineKey validationKey=\"754534E815EF6164CE788E521F845BA4953509FA45E321715FDF5B92C5BD30759C1669B4F0DABA17AC7BBF729749CE3E3203606AB49F20C19D342A078A3903D1\" decryptionKey=\"3E1AD56713339011EB29E98D1DF3DBE1BADCF353938C3429\" decryption=\"AES\" validation=\"SHA1\" />";
	        }
        }
    }
}