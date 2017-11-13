using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class UnitOfWorkAspect : IUnitOfWorkAspect
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICurrentUnitOfWork _unitOfWork;

		public UnitOfWorkAspect(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentUnitOfWork unitOfWork)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_unitOfWork = unitOfWork;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
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