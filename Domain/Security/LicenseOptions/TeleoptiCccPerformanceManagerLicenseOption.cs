using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
    /// <summary>
    /// Represents the Raptor Ass license option
    /// </summary>
    public class TeleoptiCccPerformanceManagerLicenseOption : LicenseOption
    {

        #region Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiCccPerformanceManagerLicenseOption"/> class.
        /// </summary>
        public TeleoptiCccPerformanceManagerLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccPerformanceManager, DefinedLicenseOptionNames.TeleoptiCccLifestyle)
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
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ManageScorecards),
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.AccessToPerformanceManager),
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.CreatePerformanceManagerReport),
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.ViewPerformanceManagerReport));
	    }

        #endregion
    }
}