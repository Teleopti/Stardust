using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
	public class PersianCulture : IUserSetup
	{
		public CultureInfo CultureInfo;
		public CultureInfo LanguageCultureInfo;

		public PersianCulture()
		{
			CultureInfo = CultureInfo.GetCultureInfo("fa-IR");
			LanguageCultureInfo = CultureInfo.GetCultureInfo("fa-IR");
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.SetCulture(CultureInfo);
			user.PermissionInformation.SetUICulture(LanguageCultureInfo);
			//strange - needs to be set if language pack installed
			Thread.CurrentThread.CurrentUICulture = LanguageCultureInfo;
			Thread.CurrentThread.CurrentCulture = CultureInfo;
		}
	}
}