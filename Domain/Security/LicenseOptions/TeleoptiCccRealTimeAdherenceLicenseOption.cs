using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccRealTimeAdherenceLicenseOption : LicenseOption
	{
		public TeleoptiCccRealTimeAdherenceLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccRealTimeAdherence, DefinedLicenseOptionNames.TeleoptiCccLifestyle)
		{
		}

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			EnableFunctions(
				ApplicationFunction.FindByPath(allApplicationFunctions,
					DefinedRaptorApplicationFunctionPaths.ManageRealTimeAdherence),
				ApplicationFunction.FindByPath(allApplicationFunctions,
					DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence),
				ApplicationFunction.FindByPath(allApplicationFunctions,
					DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview),
				ApplicationFunction.FindByPath(allApplicationFunctions,
					DefinedRaptorApplicationFunctionPaths.ModifyAdherence),
				ApplicationFunction.FindByPath(allApplicationFunctions,
					DefinedRaptorApplicationFunctionPaths.HistoricalOverview),
				ApplicationFunction.FindByPath(allApplicationFunctions,
					DefinedRaptorApplicationFunctionPaths.AdjustAdherence)
			);
		}
	}
}