using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public interface ITenantUnitOfWork
	{
		IDisposable Start();
		void CancelAndDisposeCurrent();
		void CommitAndDisposeCurrent();
	}
}