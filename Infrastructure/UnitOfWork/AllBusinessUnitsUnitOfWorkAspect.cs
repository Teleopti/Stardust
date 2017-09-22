using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

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
			_unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork(QueryFilter.NoFilter);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			try
			{
				if (exception == null)
					_unitOfWork.PersistAll();
			}
			finally
			{
				_unitOfWork.Dispose();
			}
		}
	}
}