using Teleopti.Ccc.Infrastructure.Security;

namespace Teleopti.Ccc.TestCommon
{
	public static class EncryptPassword
	{
		public static string ToDbFormat(string visiblePassword)
		{
			return new OneWayEncryption().CreateHash(visiblePassword);
		}
	}
}