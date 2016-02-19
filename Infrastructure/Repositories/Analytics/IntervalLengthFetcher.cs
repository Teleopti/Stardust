using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class IntervalLengthFetcher : IIntervalLengthFetcher
	{
		//have a repository or do it self
		public int IntervalLength
		{
			get
			{
				using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
				{
					return uow.Session().CreateSQLQuery(@"mart.sys_interval_length_get")
						.UniqueResult<int>();
				}
			}
		}
		private IUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
			return identity.DataSource.Analytics;
		}
	}
}