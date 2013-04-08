using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentUnitOfWork : ICurrentUnitOfWork
	{
		private readonly IUnitOfWorkFactoryProvider _unitOfWorkFactoryProvider;

		public CurrentUnitOfWork(IUnitOfWorkFactoryProvider unitOfWorkFactoryProvider)
		{
			_unitOfWorkFactoryProvider = unitOfWorkFactoryProvider;
		}

		public IUnitOfWork Current()
		{
			return _unitOfWorkFactoryProvider.LoggedOnUnitOfWorkFactory().CurrentUnitOfWork();
		}

		public static ICurrentUnitOfWork Make()
		{
			return new CurrentUnitOfWork(UnitOfWorkFactory.LoggedOnProvider());
		}
	}
}