using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public interface ITenantUnitOfWork
	{
		IDisposable EnsureUnitOfWorkIsStarted();
		void CancelAndDisposeCurrent();
		void CommitAndDisposeCurrent();
	}
}