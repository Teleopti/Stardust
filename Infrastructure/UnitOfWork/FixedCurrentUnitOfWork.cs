using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class FixedCurrentUnitOfWork : ICurrentUnitOfWork
	{
		private readonly IUnitOfWork _currentUnitOfWork;

		public FixedCurrentUnitOfWork(IUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IUnitOfWork Current()
		{
			return _currentUnitOfWork;
		}
	}
}