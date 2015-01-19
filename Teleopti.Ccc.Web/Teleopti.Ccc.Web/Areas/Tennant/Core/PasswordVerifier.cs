using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public class PasswordVerifier : IPasswordVerifier
	{
		private readonly IOneWayEncryption _oneWayEncryption;

		public PasswordVerifier(IOneWayEncryption oneWayEncryption)
		{
			_oneWayEncryption = oneWayEncryption;
		}

		public bool Check(string userPassword, string existingPasswordInDb)
		{
			return _oneWayEncryption.EncryptString(userPassword).Equals(existingPasswordInDb);
		}
	}
}