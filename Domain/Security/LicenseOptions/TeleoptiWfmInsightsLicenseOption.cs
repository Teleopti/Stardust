using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiWfmInsightsLicenseOption : LicenseOption
	{
		public TeleoptiWfmInsightsLicenseOption() : base(DefinedLicenseOptionPaths.TeleoptiWfmInsights,
			DefinedLicenseOptionNames.TeleoptiWfmVNextPilot)
		{
		}

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			var appFunctionPaths = new[]
			{
				DefinedRaptorApplicationFunctionPaths.Insights,
				DefinedRaptorApplicationFunctionPaths.DeleteInsightsReport,
				DefinedRaptorApplicationFunctionPaths.EditInsightsReport
			};

			var allFunctions = appFunctionPaths.Select(x=>ApplicationFunction.FindByPath(allApplicationFunctions, x));
			EnableFunctions(allFunctions.ToArray());
		}
	}
}