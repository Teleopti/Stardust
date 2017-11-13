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

		public AllBusinessUnitsUnitOfWorkAspect(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public AllBusinessUnitsUnitOfWorkAspect(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork(QueryFilter.NoFilter);
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