﻿using System;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkAspect : IAspect
	{
		private readonly ITenantUnitOfWorkManager _tenantUnitOfWorkManager;

		public TenantUnitOfWorkAspect(ITenantUnitOfWorkManager tenantUnitOfWorkManager)
		{
			_tenantUnitOfWorkManager = tenantUnitOfWorkManager;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			if (exception == null)
			{
				_tenantUnitOfWorkManager.CommitAndDisposeCurrent();
			}
			else
			{
				_tenantUnitOfWorkManager.CancelAndDisposeCurrent();
			}
		}
	}
}