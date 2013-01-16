using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{/// <summary>
	/// Represents the Mobile Reports license option
	/// </summary>
	public class TeleoptiCccMobileReportsLicenseOption : LicenseOption
	{

		#region Interface

		public TeleoptiCccMobileReportsLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccMobileReports, DefinedLicenseOptionNames.TeleoptiCccMobileReports)
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
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.MobileReports));
		}

		#endregion
	}
}
