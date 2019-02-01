using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class AllBusinessUnitsUnitOfWorkAspect : IAllBusinessUnitsUnitOfWorkAspect
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICurrentUnitOfWork _unitOfWork;

		public AllBusinessUnitsUnitOfWorkAspect(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentUnitOfWork unitOfWork)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_unitOfWork = unitOfWork;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			uow.DisableFilter(QueryFilter.BusinessUnit);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			try
			{
				if (exception == null)
					_unitOfWork.Current().PersistAll();
			}
			finally
			{
				_unitOfWork.Current().Dispose();
			}
		}
	}
}