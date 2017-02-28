﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
    /// <summary>
    /// Represents the Early Ams license option
    /// </summary>
    public class TeleoptiCccPilotCustomersSchedulerLicenseOption : LicenseOption
    {

        #region Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiCccPilotCustomersSchedulerLicenseOption"/> class.
        /// </summary>
        public TeleoptiCccPilotCustomersSchedulerLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersScheduler, DefinedLicenseOptionNames.TeleoptiCccPilotCustomersBase)
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
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence),
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment),
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OpenSchedulePage),
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules),
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.AutomaticScheduling),
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.RequestScheduler),
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove));
	    }

        #endregion
    }
}