using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class IntervalLengthFetcher : IIntervalLengthFetcher
	{
		private readonly ICurrentDataSource _currentDataSource;

		public IntervalLengthFetcher(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public int GetIntervalLength()
		{
			using (IStatelessUnitOfWork uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(@"mart.sys_interval_length_get")
					.UniqueResult<int>();
			}
		}
	}
}