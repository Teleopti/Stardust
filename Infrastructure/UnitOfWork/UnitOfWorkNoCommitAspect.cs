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
			var session = uow.Session();
			if (session == null)
				return; //to avoid nullex in domain tests
			session.DefaultReadOnly = true;
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_currentUnitOfWork.Current().Dispose();
		}
	}
}