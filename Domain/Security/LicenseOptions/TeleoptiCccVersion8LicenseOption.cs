using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccVersion8LicenseOption : LicenseOption
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TeleoptiCccShiftTraderLicenseOption"/> class.
		/// </summary>
		public TeleoptiCccVersion8LicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccVersion8, DefinedLicenseOptionNames.TeleoptiCccVersion8)
		{
		}

		/// <summary>
		/// Sets all application functions.
		/// </summary>
		/// <param name="allApplicationFunctions"></param>
		/// <value>The enabled application functions.</value>
		public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
		{
			EnabledApplicationFunctions.Clear();

			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.IntradayReForecasting));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount));
		}
	}
}