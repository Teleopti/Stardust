using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Security
{
    /// <summary>
    /// Testable StaticAvailableDataProvider class.
    /// </summary>
    public class StaticAvailableDataProviderTestClass : StaticAvailableDataProvider
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticAvailableDataProviderTestClass"/> class.
        /// </summary>
        /// <param name="availableDataRepository">The available data repository.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public StaticAvailableDataProviderTestClass(IAvailableDataRepository availableDataRepository) : base(availableDataRepository)
        {
            //
        }

        /// <summary>
        /// Makes the underlying method testable.
        /// </summary>
        /// <value>The input application roles.</value>
        public new IList<IApplicationRole> InputApplicationRoles
        {
            get { return base.InputApplicationRoles; }
        }
    }
}
