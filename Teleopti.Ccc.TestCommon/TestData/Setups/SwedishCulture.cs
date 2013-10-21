using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups
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