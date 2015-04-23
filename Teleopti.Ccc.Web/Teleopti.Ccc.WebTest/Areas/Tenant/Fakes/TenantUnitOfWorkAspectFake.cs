using System;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Fakes
{
	public class TenantUnitOfWorkAspectFake : ITenantUnitOfWorkAspect
	{
		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			LastCommitSucceded = exception == null;
		}

		public bool? LastCommitSucceded { get; private set; }
	}
}