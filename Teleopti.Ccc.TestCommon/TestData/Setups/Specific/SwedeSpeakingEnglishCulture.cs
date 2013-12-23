using System.Globalization;
using System.Threading;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
	public class SwedeSpeakingEnglishCulture : IUserSetup
	{
		public CultureInfo CultureInfo;
		public CultureInfo LanguageCultureInfo;

		public SwedeSpeakingEnglishCulture()
		{
			CultureInfo = CultureInfo.GetCultureInfo("sv-SE");
			LanguageCultureInfo = CultureInfo.GetCultureInfo("en-GB");
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