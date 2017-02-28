using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccCalendarLinkLicenseOption : LicenseOption
	{
		public TeleoptiCccCalendarLinkLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccCalendarLink, DefinedLicenseOptionNames.TeleoptiCccCalendarLink)
		{
		}

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			EnableFunctions(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShareCalendar));
		}
	}
}