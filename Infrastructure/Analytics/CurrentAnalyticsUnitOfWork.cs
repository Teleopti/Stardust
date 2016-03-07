using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public class CurrentAnalyticsUnitOfWork : ICurrentAnalyticsUnitOfWork
	{
		private readonly ICurrentAnalyticsUnitOfWorkFactory _unitOfWorkFactory;

		public static ICurrentAnalyticsUnitOfWork Make()
		{
			return new CurrentAnalyticsUnitOfWork(CurrentAnalyticsUnitOfWorkFactory.Make());
		}

		public CurrentAnalyticsUnitOfWork(ICurrentAnalyticsUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IUnitOfWork Current()
		{
			return _unitOfWorkFactory.Current().CurrentUnitOfWork();
		}
	}
}