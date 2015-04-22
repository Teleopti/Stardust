namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public interface ITenantUnitOfWorkManager
	{
		void CancelAndDisposeCurrent();
		void CommitAndDisposeCurrent();
	}
}