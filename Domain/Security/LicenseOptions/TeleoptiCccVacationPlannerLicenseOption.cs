using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

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
		    EnableFunctions(
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove),
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.AbsenceRequests),
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb),
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.RequestAllowances),
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount),
				ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.WebCancelRequest));
	    }

        #endregion
    }
}