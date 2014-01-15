using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    /// <summary>
    /// Represents the license option used for testing
    /// </summary>
    public class AllLicenseOption : LicenseOption
    {
        public const string FakeOptionPath = "FakeThatShouldNotBeUsed";

        #region Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="AllLicenseOption"/> class.
        /// </summary>
        public AllLicenseOption()
            : base(FakeOptionPath, "All")
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
            foreach (ApplicationFunction applicationFunction in allApplicationFunctions)
            {
                //if ((applicationFunction.Level <= 1) || (applicationFunction.ForeignSource == DefinedForeignSourceNames.SourceMatrix))
                EnabledApplicationFunctions.Add(applicationFunction);
            }
        }

        #endregion
    }
}
