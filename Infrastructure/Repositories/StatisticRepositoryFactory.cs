using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

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
			ITeleoptiIdentity identity = null;
			var principal = TeleoptiPrincipal.CurrentPrincipal;
			if (principal!=null)
			{
				identity = principal.Identity as ITeleoptiIdentity;
			}
        	if (identity == null || identity.DataSource.Analytics == null)
                return new StatisticRepositoryEmpty();
            
            return new StatisticRepository();
        }

		  public static IAnalyticsScheduleRepository CreateAnalyticsSchedule()
		  {
			  ITeleoptiIdentity identity = null;
			  var principal = TeleoptiPrincipal.CurrentPrincipal;
			  if (principal != null)
			  {
				  identity = principal.Identity as ITeleoptiIdentity;
			  }
			  if (identity == null || identity.DataSource.Analytics == null)
				  return null; //have empty here too
 
			  return new AnalyticsScheduleRepository();
		  }

			public static IIntervalLengthFetcher CreateAnalyticsIntervalLength()
			{
				ITeleoptiIdentity identity = null;
				var principal = TeleoptiPrincipal.CurrentPrincipal;
				if (principal != null)
				{
					identity = principal.Identity as ITeleoptiIdentity;
				}
				if (identity == null || identity.DataSource.Analytics == null)
					return null; //have empty here too

				return new IntervalLengthFetcher();
			}
    }
}
