using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiWfmOutboundLicenseOption : LicenseOption
	{
		public TeleoptiWfmOutboundLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiWfmOutbound, DefinedLicenseOptionNames.TeleoptiWfmVNextPilot)
		{
		}

		public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
		{
			EnabledApplicationFunctions.Clear();
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.Outbound));
		}
	}
}
