using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
	public class BulgarianCulture : IUserSetup
	{
		public CultureInfo CultureInfo;

		public BulgarianCulture()
		{
			CultureInfo = CultureInfo.GetCultureInfo("bg-BG");
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
