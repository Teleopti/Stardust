using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
    /// <summary>
    /// Represents the Early Ams license option
    /// </summary>
    public class TeleoptiCccPilotCustomersPermissionsLicenseOption : LicenseOption
    {

        #region Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiCccPilotCustomersPermissionsLicenseOption"/> class.
        /// </summary>
        public TeleoptiCccPilotCustomersPermissionsLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersPermissions, DefinedLicenseOptionNames.TeleoptiCccPilotCustomersBase)
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
        }

        #endregion
    }
}