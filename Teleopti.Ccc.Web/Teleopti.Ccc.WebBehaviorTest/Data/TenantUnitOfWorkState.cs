using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TenantUnitOfWorkState
	{
		private static TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		private static TenantUnitOfWorkManager tenantUnitOfWorkManager()
		{
			return _tenantUnitOfWorkManager ?? 
			       (_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(UnitOfWorkFactory.Current.ConnectionString));
		}

		public static void TenantUnitOfWorkAction(Action<ICurrentTenantSession> action)
		{
			var tenantUowManager = tenantUnitOfWorkManager();
			using (tenantUowManager.EnsureUnitOfWorkIsStarted())
			{
				action.Invoke(tenantUowManager);
			}
		}
	}
}