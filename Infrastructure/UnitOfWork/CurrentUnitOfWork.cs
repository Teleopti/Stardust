using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentUnitOfWork : ICurrentUnitOfWork
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public CurrentUnitOfWork(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IUnitOfWork Current()
		{
			return _unitOfWorkFactory.CurrentUnitOfWork();
		}

		public static ICurrentUnitOfWork Make()
		{
			return new CurrentUnitOfWork(UnitOfWorkFactory.Current);
		}
	}
}