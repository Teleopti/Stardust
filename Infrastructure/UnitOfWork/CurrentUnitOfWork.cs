using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentUnitOfWork : ICurrentUnitOfWork
	{
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly Func<IUnitOfWorkFactory> _unitOfWorkFactoryGetter;

		public CurrentUnitOfWork(Func<IUnitOfWorkFactory> unitOfWorkFactory)
		{
			_unitOfWorkFactoryGetter = unitOfWorkFactory;
		}

		public IUnitOfWork Current()
		{
			if (_unitOfWorkFactory == null)
				_unitOfWorkFactory = _unitOfWorkFactoryGetter.Invoke();
			return _unitOfWorkFactory.CurrentUnitOfWork();
		}

		public static ICurrentUnitOfWork Make()
		{
			return new CurrentUnitOfWork(() => UnitOfWorkFactory.Current);
		}
	}
}