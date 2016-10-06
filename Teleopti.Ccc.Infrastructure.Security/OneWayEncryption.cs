using System;
using System.Security.Cryptography;
using System.Text;

namespace Teleopti.Ccc.Infrastructure.Security
{
	public class OneWayEncryption : IHashFunction
	{
		private const string salt = "adgvabar4g61qt46gv";

		public string CreateHash(string password)
		{
			var stringValue = string.Concat(salt, password);
			byte[] hashedBytes;
			using (SHA1Managed encryptor = new SHA1Managed())
			{
				hashedBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(stringValue));
			}
			return string.Concat("###", BitConverter.ToString(hashedBytes).Replace("-", ""), "###");
		}

		public bool Verify(string password, string hash)
		{
			return hash == CreateHash(password);
		}

		public bool IsGeneratedByThisFunction(string hash)
		{
			return hash.StartsWith("###") && hash.EndsWith("###");
		}
	}
}
