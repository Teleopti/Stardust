using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
	public class USCulture : IUserSetup
	{
		public CultureInfo CultureInfo;

		public USCulture()
		{
			CultureInfo = CultureInfo.GetCultureInfo("en-US");
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.SetCulture(CultureInfo);
			user.PermissionInformation.SetUICulture(CultureInfo);
			//strange - needs to be set if language pack installed
			Thread.CurrentThread.CurrentUICulture = CultureInfo;
			Thread.CurrentThread.CurrentCulture = CultureInfo;
		}
	}
}