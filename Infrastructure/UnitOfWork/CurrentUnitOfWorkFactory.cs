using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentUnitOfWorkFactory : ICurrentUnitOfWorkFactory
	{
		private readonly ICurrentDataSource _currentDataSource;

		public static ICurrentUnitOfWorkFactory Make() => new CurrentUnitOfWorkFactory(CurrentDataSource.Make());

		public CurrentUnitOfWorkFactory(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public IUnitOfWorkFactory Current() => _currentDataSource.Current()?.Application;
	}
}