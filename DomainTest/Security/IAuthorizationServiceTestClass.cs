using Teleopti.Ccc.Domain.Security.Accessories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security
{
    /// <summary>
    /// Interface for the AuthorizationServiceTestClass
    /// </summary>
    public interface IAuthorizationServiceTestClass : IAuthorizationService
    {
        /// <summary>
        /// Gets the person period cache.
        /// </summary>
        /// <value>The person period cache.</value>
        IPersonPeriodDynamicCache ThePersonPeriodCache { get;set;}

        /// <summary>
        /// Gets or sets the aggregated permitted data cache.
        /// </summary>
        /// <value>The aggregated permitted data cache.</value>
        IAggregatedPermittedDataDynamicCache TheAggregatedPermittedDataCache { get; set; }
    }
}
