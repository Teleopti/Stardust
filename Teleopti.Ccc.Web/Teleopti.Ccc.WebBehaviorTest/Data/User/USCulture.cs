using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class USCulture : IUserSetup
	{
		public CultureInfo CultureInfo;

		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			CultureInfo = CultureInfo.GetCultureInfo("en-US");
			user.PermissionInformation.SetCulture(CultureInfo);
			user.PermissionInformation.SetUICulture(CultureInfo);
		}
	}
}