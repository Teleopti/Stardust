using System.Globalization;
using System.Threading;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
	public class IranianCulture : IUserSetup
	{
		public CultureInfo CultureInfo;

		public IranianCulture()
		{
			CultureInfo = CultureInfo.GetCultureInfo("fa-IR");
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