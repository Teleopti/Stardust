﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
    /// <summary>
    /// Represents the Raptor Ass license option
    /// </summary>
    public class TeleoptiCccVacationPlannerLicenseOption : LicenseOption
    {

        #region Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiCccVacationPlannerLicenseOption"/> class.
        /// </summary>
        public TeleoptiCccVacationPlannerLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccVacationPlanner, DefinedLicenseOptionNames.TeleoptiCccLifestyle)
        {
            //
        }

	    /// <summary>
	    /// Sets all application functions.
	    /// </summary>
	    /// <param name="allApplicationFunctions"></param>
	    /// <value>The enabled application functions.</value>
	    public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
        {
            EnabledApplicationFunctions.Clear();
            EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove));
            EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.AbsenceRequests));
            EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb));
            EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.RequestAllowances));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount));
		}

        #endregion
    }
}