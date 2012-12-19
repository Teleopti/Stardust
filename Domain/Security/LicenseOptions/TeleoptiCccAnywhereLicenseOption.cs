﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{/// <summary>
	/// Represents the Anywhere license option
	/// </summary>
	public class TeleoptiCccAnywhereLicenseOption : LicenseOption
	{

		#region Interface

		public TeleoptiCccAnywhereLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccAnywhere, DefinedLicenseOptionNames.TeleoptiCccAnywhere)
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
