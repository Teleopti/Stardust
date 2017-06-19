using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
    /// <summary>
    /// Represents the Raptor Ass license option
    /// </summary>
    public class TeleoptiCccShiftTraderLicenseOption : LicenseOption
    {
	    /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiCccShiftTraderLicenseOption"/> class.
        /// </summary>
        public TeleoptiCccShiftTraderLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccShiftTrader, DefinedLicenseOptionNames.TeleoptiCccLifestyle)
        {
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
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequests),
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb),
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard));
	    }
    }
}