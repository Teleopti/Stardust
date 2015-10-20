using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiWfmVNextPilotLicenseOption : LicenseOption
	{
		public TeleoptiWfmVNextPilotLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiWfmVNextPilot, DefinedLicenseOptionNames.TeleoptiWfmVNextPilot)
		{
		}

		public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
		{
			EnabledApplicationFunctions.Clear();
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.WebForecasts));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.WebPermissions));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.WebSchedules));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.WebPeople));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.WebModifySkill));
		}
	}
}