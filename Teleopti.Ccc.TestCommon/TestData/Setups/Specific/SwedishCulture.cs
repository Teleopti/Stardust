using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
	public class SwedishCultureSpec
	{
		public CultureInfo CultureInfo = CultureInfo.GetCultureInfo("sv-SE");
	}

	public class SwedishCultureSetup : IUserSetup<SwedishCultureSpec>
	{
		public void Apply(SwedishCultureSpec spec, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.SetCulture(spec.CultureInfo);
			user.PermissionInformation.SetUICulture(spec.CultureInfo);
			//strange - needs to be set if language pack installed
			Thread.CurrentThread.CurrentUICulture = spec.CultureInfo;
			Thread.CurrentThread.CurrentCulture = spec.CultureInfo;
		}
	}
}