using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiWfmOvertimeRequestsLicenseOption : LicenseOption
	{
		public TeleoptiWfmOvertimeRequestsLicenseOption(): base(DefinedLicenseOptionPaths.TeleoptiWfmOvertimeRequests, DefinedLicenseOptionNames.TeleoptiWfmVNextPilot)
		{
		}

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			var applicationFunctions = allApplicationFunctions.ToList();
			EnableFunctions(ApplicationFunction.FindByPath(applicationFunctions, DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb), ApplicationFunction.FindByPath(applicationFunctions, DefinedRaptorApplicationFunctionPaths.WebOvertimeRequest));
		}
	}
}
