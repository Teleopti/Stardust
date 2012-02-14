using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
    /// <summary>
    /// Represents a developer license option
    /// </summary>
    public class TeleoptiCccDeveloperLicenseOption : LicenseOption
    {

        #region Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiCccBaseLicenseOption"/> class.
        /// </summary>
        public TeleoptiCccDeveloperLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccDeveloper, DefinedLicenseOptionNames.TeleoptiCccDeveloper)
        {
            //
        }

        /// <summary>
        /// Sets the enabled (licensed) application functions.
        /// </summary>
        /// <param name="allApplicationFunctions">All application functions.</param>
        /// <value>The enabled application functions.</value>
        public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
        {
            EnabledApplicationFunctions.Clear();
            
            foreach (IApplicationFunction applicationFunction in new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList)
            {
                if (applicationFunction.IsPreliminary)
                    EnabledApplicationFunctions.Add(applicationFunction);
                applicationFunction.IsPreliminary = false;
            }
            foreach (IApplicationFunction applicationFunction in allApplicationFunctions)
            {
                applicationFunction.IsPreliminary = false;
            }
        }

        #endregion
    }
}