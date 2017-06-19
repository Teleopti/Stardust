using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	/// <summary>
	/// Represents the Freemium base license option
	/// </summary>
	public class TeleoptiCccFreemiumForecastsLicenseOption : LicenseOption
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TeleoptiCccFreemiumForecastsLicenseOption"/> class.
		/// </summary>
		public TeleoptiCccFreemiumForecastsLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccFreemiumForecasts, DefinedLicenseOptionNames.TeleoptiCccFreemiumBase)
		{
			//
		}

		/// <summary>
		/// Sets the enabled (licensed) application functions.
		/// </summary>
		/// <param name="allApplicationFunctions">All application functions.</param>
		/// <value>The enabled application functions.</value>
		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			EnableFunctions(
				ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication),
				ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.RaptorGlobal),
				ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OpenForecasterPage),
				ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ExportForecastFile),
				ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OpenBudgets),
				ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.WebForecasts),
				ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.WebModifySkill),
				ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.Anywhere));
		}
	}
}