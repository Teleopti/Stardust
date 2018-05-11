using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class UnitOfWorkNoCommitAspect : IUnitOfWorkNoCommitAspect
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public UnitOfWorkNoCommitAspect(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_currentUnitOfWork = currentUnitOfWork;
		}


		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			uow.Session().DefaultReadOnly = true;
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_currentUnitOfWork.Current().Dispose();
		}
	}
	
	public class UnitOfWorkNoCommitAspectOLD : IUnitOfWorkNoCommitAspect
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public UnitOfWorkNoCommitAspectOLD(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_currentUnitOfWork = currentUnitOfWork;
		}


		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_currentUnitOfWork.Current().Dispose();
		}
	}
}