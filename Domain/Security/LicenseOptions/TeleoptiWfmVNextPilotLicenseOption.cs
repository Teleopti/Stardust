using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

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
				DefinedRaptorApplicationFunctionPaths.WebModifySkill
			};

			var all = allApplicationFunctions.ToList();
			EnableFunctions(applicationFunctions.Select(func => ApplicationFunction.FindByPath(all, func)).ToArray());
		}
	}
}