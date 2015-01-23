using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public class PasswordVerifier : IPasswordVerifier
	{
		private readonly IOneWayEncryption _oneWayEncryption;

		public PasswordVerifier(IOneWayEncryption oneWayEncryption)
		{
			_oneWayEncryption = oneWayEncryption;
		}

		public bool Check(string userPassword, PasswordPolicyForUser passwordPolicyForUser)
		{
			return passwordPolicyForUser.ValidPassword(_oneWayEncryption.EncryptString(userPassword));
		}
	}
}