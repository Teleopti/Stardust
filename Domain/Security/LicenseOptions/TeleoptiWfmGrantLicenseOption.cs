using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiWfmGrantLicenseOption : LicenseOption
	{
		public TeleoptiWfmGrantLicenseOption() : base(DefinedLicenseOptionPaths.TeleoptiWfmGrant, DefinedLicenseOptionNames.TeleoptiWfmVNextPilot)
		{
		}

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			var applicationFunctions = allApplicationFunctions.ToList();
			EnableFunctions(ApplicationFunction.FindByPath(applicationFunctions, DefinedRaptorApplicationFunctionPaths.ChatBot));
		}
	}
}