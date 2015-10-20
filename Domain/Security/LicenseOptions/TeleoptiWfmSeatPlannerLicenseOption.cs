using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiWfmSeatPlannerLicenseOption : LicenseOption
	{
		public TeleoptiWfmSeatPlannerLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiWfmSeatPlanner, DefinedLicenseOptionNames.TeleoptiWfmVNextPilot)
		{
		}

		public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
		{
			EnabledApplicationFunctions.Clear();
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.SeatPlanner));
		}
	}
}
