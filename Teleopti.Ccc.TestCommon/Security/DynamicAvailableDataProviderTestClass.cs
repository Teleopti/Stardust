using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Security
{
    /// <summary>
    /// Testable DynamicAvailableDataProviderTestClass class.
    /// </summary>
    public class DynamicAvailableDataProviderTestClass : DynamicAvailableDataProvider
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticAvailableDataProviderTestClass"/> class.
        /// </summary>
        /// <param name="availableDataRepository">The available data repository.</param>
        /// <param name="businessUnitRepository">The business unit repository.</param>
        /// <param name="person">The person.</param>
        /// <param name="currentBusinessUnit">The business unit.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public DynamicAvailableDataProviderTestClass(IAvailableDataRepository availableDataRepository, IBusinessUnitRepository businessUnitRepository, IPerson person, IBusinessUnit currentBusinessUnit) : base(availableDataRepository, businessUnitRepository, person, currentBusinessUnit)
        {
        }

        /// <summary>
        /// Makes the underlying method testable.
        /// </summary>
        /// <value>The input application roles.</value>
        public new IList<IApplicationRole> InputApplicationRoles
        {
            get { return base.InputApplicationRoles; }
        }

        /// <summary>
        /// Makes base method testable.
        /// </summary>
        /// <param name="availableDataRange">The available data range.</param>
        /// <param name="applicationRole"></param>
        /// <param name="person">The person.</param>
        /// <param name="businessUnit">The business unit.</param>
        /// <param name="businessUnitRepository">The business unit repository.</param>
        /// <returns></returns>
        public new IAvailableData CreateDynamicAvailableData(AvailableDataRangeOption availableDataRange, IApplicationRole applicationRole, IPerson person, IBusinessUnit businessUnit, IBusinessUnitRepository businessUnitRepository)
        {
            return base.CreateDynamicAvailableData(availableDataRange, applicationRole, person, businessUnit, businessUnitRepository);
        }

        /// <summary>
        /// Makes base method testable.
        /// </summary>
        /// <param name="dynamicAvailableData">The dynamic available data.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public new IAvailableData AddMyOwn(IAvailableData dynamicAvailableData, IPerson person)
        {
            return base.AddMyOwn(dynamicAvailableData, person);
        }

        /// <summary>
        /// Makes base method testable.
        /// </summary>
        /// <param name="dynamicAvailableData">The dynamic available data.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public new IAvailableData AddMyTeam(IAvailableData dynamicAvailableData, IPerson person)
        {
            return base.AddMyTeam(dynamicAvailableData, person);
        }

        /// <summary>
        /// Makes base method testable.
        /// </summary>
        /// <param name="dynamicAvailableData">The dynamic available data.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public new IAvailableData AddMySite(IAvailableData dynamicAvailableData, IPerson person)
        {
            return base.AddMySite(dynamicAvailableData, person);
        }

        /// <summary>
        /// Makes base method testable.
        /// </summary>
        /// <param name="dynamicAvailableData">The dynamic available data.</param>
        /// <param name="businessUnit">The business unit.</param>
        /// <returns></returns>
        public new IAvailableData AddMyBusinessUnit(IAvailableData dynamicAvailableData, IBusinessUnit businessUnit)
        {
            return base.AddMyBusinessUnit(dynamicAvailableData, businessUnit);
        }


    }
}
