using System;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class TenantUnitOfWorkAspectFake : ITenantUnitOfWorkAspect
	{
		private Exception _exceptionToThrow;

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			if (_exceptionToThrow != null)
				throw _exceptionToThrow;
		
			CommitSucceded = exception == null;
		}

		public bool CommitSucceded { get; private set; }

		public void WillThrow(Exception exceptionToThrow)
		{
			_exceptionToThrow = exceptionToThrow;
		}
	}
}