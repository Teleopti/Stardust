using System;
using Teleopti.Interfaces.Infrastructure;

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
			return _unitOfWorkFactory().HasCurrentUnitOfWork();
		}

		public IUnitOfWork Current()
		{
			return _unitOfWorkFactory().CurrentUnitOfWork();
		}
	}
}