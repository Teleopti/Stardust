using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class SwedishCulture : IUserSetup
	{
		public CultureInfo CultureInfo;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			CultureInfo = CultureInfo.GetCultureInfo("sv-SE");
			user.PermissionInformation.SetCulture(CultureInfo);
			user.PermissionInformation.SetUICulture(CultureInfo);
		}
	}
}