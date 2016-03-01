using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class WithTenantUnitOfWork
	{
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ICurrentTenantSession _tenantSession;

		public WithTenantUnitOfWork(ITenantUnitOfWork tenantUnitOfWork, ICurrentTenantSession tenantSession)
		{
			_tenantUnitOfWork = tenantUnitOfWork;
			_tenantSession = tenantSession;
		}

		public void Do(Action<ICurrentTenantSession> action)
		{
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				action.Invoke(_tenantSession);
				_tenantUnitOfWork.CommitAndDisposeCurrent();
			}
		}
	}
}