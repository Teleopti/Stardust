using System.Collections.Generic;
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
        public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
        {
            EnabledApplicationFunctions.Clear();
            EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ManageScorecards));
            EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OpenScorecard));
            EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.AccessToPerformanceManager));
            EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.CreatePerformanceManagerReport));
            EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(allApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewPerformanceManagerReport));
        }

        #endregion
    }
}