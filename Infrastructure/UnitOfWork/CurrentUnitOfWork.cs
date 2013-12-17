using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentUnitOfWork : ICurrentUnitOfWork
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public CurrentUnitOfWork(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public IUnitOfWork Current()
		{
			return _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CurrentUnitOfWork();
		}

		public static ICurrentUnitOfWork Make()
		{
			return new CurrentUnitOfWork(UnitOfWorkFactory.CurrentUnitOfWorkFactory());
		}
	}
}