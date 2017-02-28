using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccOvertimeAvailabilityLicenseOption : LicenseOption
	{
		public TeleoptiCccOvertimeAvailabilityLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccOvertimeAvailability, DefinedLicenseOptionNames.TeleoptiCccLifestyle)
		{
		}

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			EnableFunctions(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb));
		}
	}
}