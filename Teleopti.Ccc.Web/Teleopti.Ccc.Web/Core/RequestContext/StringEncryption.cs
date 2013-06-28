using System.Web.Security;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public static class StringEncryption
	{

		public static string Encrypt(string stringToEncrypt)
		{

			return Encryption.EncryptStringToBase64(stringToEncrypt, EncryptionConstants.Image1, EncryptionConstants.Image2);
		}

		public static string Decrypt(string encryptedString)
		{
			return Encryption.DecryptStringFromBase64(encryptedString, EncryptionConstants.Image1, EncryptionConstants.Image2);
		}
	}
}