using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class FromFactory : ICurrentUnitOfWork
	{
		private readonly Func<IUnitOfWorkFactory> _unitOfWorkFactory;

		public FromFactory(Func<IUnitOfWorkFactory> unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public bool HasCurrent()
		{
			var unitOfWorkFactory = _unitOfWorkFactory();
			return unitOfWorkFactory != null && unitOfWorkFactory.HasCurrentUnitOfWork();
		}

		public IUnitOfWork Current()
		{
			return _unitOfWorkFactory().CurrentUnitOfWork();
		}
	}
}