using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.WebTest.Areas.Tennant.Core
{
	public static class EncryptPassword
	{
		public static string ToDbFormat(string visiblePassword)
		{
			return new OneWayEncryption().EncryptString(visiblePassword);
		}
	}
}