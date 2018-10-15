using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiWfmPmNextGenLicenseOption : LicenseOption
	{
		public TeleoptiWfmPmNextGenLicenseOption() : base(DefinedLicenseOptionPaths.TeleoptiWfmPmNextGen,
			DefinedLicenseOptionNames.TeleoptiWfmVNextPilot)
		{
		}

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			var appFunctionPathes = new[]
			{
				DefinedRaptorApplicationFunctionPaths.PmNextGen,
				DefinedRaptorApplicationFunctionPaths.PmNextGenViewReport,
				DefinedRaptorApplicationFunctionPaths.PmNextGenEditReport
			};

			var allFunctions = appFunctionPathes.Select(x=>ApplicationFunction.FindByPath(allApplicationFunctions, x));
			EnableFunctions(allFunctions.ToArray());
		}
	}
}