﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	/// <summary>
    /// Represents the Early Ams license option
    /// </summary>
    public class TeleoptiCccPilotCustomersBaseLicenseOption : LicenseOption
    {

        #region Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiCccPilotCustomersBaseLicenseOption"/> class.
        /// </summary>
        public TeleoptiCccPilotCustomersBaseLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersBase, DefinedLicenseOptionNames.TeleoptiCccPilotCustomersBase)
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
				ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.RaptorGlobal));
		}

        #endregion
    }
}