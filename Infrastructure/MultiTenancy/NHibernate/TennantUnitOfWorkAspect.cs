using System;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public class TennantUnitOfWorkAspect : IAspect
	{
		private readonly ITenantUnitOfWorkManager _tenantUnitOfWorkManager;

		public TennantUnitOfWorkAspect(ITenantUnitOfWorkManager tenantUnitOfWorkManager)
		{
			_tenantUnitOfWorkManager = tenantUnitOfWorkManager;
		}

		public void OnBeforeInvokation()
		{
		}

		public void OnAfterInvokation(Exception exception)
		{
			if (exception == null)
			{
				_tenantUnitOfWorkManager.CommitCurrent();
			}
			else
			{
				_tenantUnitOfWorkManager.CancelCurrent();
			}
		}
	}
}