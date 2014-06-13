using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccOvertimeAvailabilityLicenseOption : LicenseOption
	{
		public TeleoptiCccOvertimeAvailabilityLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccOvertimeAvailability, DefinedLicenseOptionNames.TeleoptiCccAss)
		{
			
		}

		public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
		{
			EnabledApplicationFunctions.Clear();
		}
	}
}