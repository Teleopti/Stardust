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
			var applicationFunctions = new[]
			{
				DefinedRaptorApplicationFunctionPaths.WebForecasts,
				DefinedRaptorApplicationFunctionPaths.WebPermissions,
				DefinedRaptorApplicationFunctionPaths.WebSchedules,
				DefinedRaptorApplicationFunctionPaths.WebPeople,
				DefinedRaptorApplicationFunctionPaths.WebRequests,
				DefinedRaptorApplicationFunctionPaths.WebModifySkill,
				DefinedRaptorApplicationFunctionPaths.WebIntraday,
				DefinedRaptorApplicationFunctionPaths.WebModifySkillArea,
				DefinedRaptorApplicationFunctionPaths.AngelMyTeamSchedules
			};

			EnabledApplicationFunctions.Clear();
			foreach (var func in applicationFunctions)
			{
				EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, func));
			}
		}
	}
}