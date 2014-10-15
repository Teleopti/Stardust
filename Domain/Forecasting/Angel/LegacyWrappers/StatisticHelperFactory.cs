using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers
{
	public class StatisticHelperFactory : IStatisticHelperFactory
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public StatisticHelperFactory(ICurrentUnitOfWork currentUnitOfWork, IRepositoryFactory repositoryFactory)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_repositoryFactory = repositoryFactory;
		}

		public IStatisticHelper Create()
		{
			return new StatisticHelper(_repositoryFactory, _currentUnitOfWork.Current());
		}
	}
}