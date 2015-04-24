using System;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkAspect : ITenantUnitOfWorkAspect
	{
		private readonly ITenantUnitOfWork _tenantUnitOfWork;

		public TenantUnitOfWorkAspect(ITenantUnitOfWork tenantUnitOfWork)
		{
			_tenantUnitOfWork = tenantUnitOfWork;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			if (exception == null)
			{
				_tenantUnitOfWork.CommitAndDisposeCurrent();
			}
			else
			{
				_tenantUnitOfWork.CancelAndDisposeCurrent();
			}
		}
	}
}