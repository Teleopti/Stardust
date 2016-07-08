using System.Collections.Generic;
using System.Linq;
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

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			var applicationFunctions = new[]
			{
				DefinedRaptorApplicationFunctionPaths.WebForecasts,
				DefinedRaptorApplicationFunctionPaths.WebSchedules,
				DefinedRaptorApplicationFunctionPaths.WebCancelRequest,
				DefinedRaptorApplicationFunctionPaths.WebModifySkill
			};

			var all = allApplicationFunctions.ToList();
			EnabledApplicationFunctions.Clear();
			foreach (var func in applicationFunctions)
			{
				EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, func));
			}
		}
	}
}