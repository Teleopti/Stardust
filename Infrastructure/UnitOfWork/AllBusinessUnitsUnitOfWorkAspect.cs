using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class AllBusinessUnitsUnitOfWorkAspect : IAllBusinessUnitsUnitOfWorkAspect
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IUnitOfWork _unitOfWork;

		public AllBusinessUnitsUnitOfWorkAspect(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_unitOfWork = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork(QueryFilter.NoFilter);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			if (exception == null)
				_unitOfWork.PersistAll();
			_unitOfWork.Dispose();
		}
	}
}