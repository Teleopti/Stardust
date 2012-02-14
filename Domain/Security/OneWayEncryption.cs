using System;
using System.Security.Cryptography;
using System.Text;

namespace Teleopti.Ccc.Domain.Security
{
    public class OneWayEncryption : IOneWayEncryption
    {
        private const string Salt = "adgvabar4g61qt46gv";

        public string EncryptString(string value)
        {
        	return EncryptString(value, Salt);
        }

		public string EncryptString(string value, string salt)
		{
			var stringValue = string.Concat(Salt, value);
			byte[] hashedBytes;
			using (SHA1Managed encryptor = new SHA1Managed())
			{
				hashedBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(stringValue));
			}
			return string.Concat("###", BitConverter.ToString(hashedBytes).Replace("-", ""), "###");
		}

        public string EncryptStringWithBase64(string value, string salt)
        {
            var stringValue = string.Concat(salt, value);
            byte[] hashedBytes;
            using (var encryptor = new SHA256Managed())
            {
                hashedBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(stringValue));
            }
            return Convert.ToBase64String(hashedBytes);
        }
    }
}