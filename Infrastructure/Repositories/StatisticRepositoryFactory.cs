using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Factory for StatisticRepository.
    /// Tested from tests for repositories
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-04-29
    /// </remarks>
    public static class StatisticRepositoryFactory
    {
        /// <summary>
        /// Creates a repository instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-29
        /// </remarks>
        public static IStatisticRepository Create()
        {
            var identity = ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
            if (identity.DataSource.Statistic == null)
                return new StatisticRepositoryEmpty();
            
            return new StatisticRepository();
        }
    }
}
