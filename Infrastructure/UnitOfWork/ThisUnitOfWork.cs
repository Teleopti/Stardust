using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ThisUnitOfWork : ICurrentUnitOfWork
	{
		private readonly IUnitOfWork _currentUnitOfWork;

		public ThisUnitOfWork(IUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IUnitOfWork Current()
		{
			return _currentUnitOfWork;
		}
	}
}