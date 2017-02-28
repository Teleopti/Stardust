using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
    /// <summary>
    /// Represents the Early Ams license option
    /// </summary>
    public class TeleoptiCccPilotCustomersPeopleLicenseOption : LicenseOption
    {

        #region Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiCccPilotCustomersPeopleLicenseOption"/> class.
        /// </summary>
        public TeleoptiCccPilotCustomersPeopleLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersPeople, DefinedLicenseOptionNames.TeleoptiCccPilotCustomersBase)
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
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage),
			    ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ModifyGroupPage),
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.ModifyPeopleWithinGroupPage),
			    ApplicationFunction.FindByPath(allApplicationFunctions,
				    DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword));
	    }

        #endregion
    }
}