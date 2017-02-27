﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
    /// <summary>
    /// Represents the Early Ams license option
    /// </summary>
    public class TeleoptiCccPilotCustomersForecastsLicenseOption : LicenseOption
    {

        #region Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiCccPilotCustomersForecastsLicenseOption"/> class.
        /// </summary>
        public TeleoptiCccPilotCustomersForecastsLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersForecasts, DefinedLicenseOptionNames.TeleoptiCccPilotCustomersBase)
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
            EnableFunctions(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OpenForecasterPage));
        }

        #endregion
    }
}