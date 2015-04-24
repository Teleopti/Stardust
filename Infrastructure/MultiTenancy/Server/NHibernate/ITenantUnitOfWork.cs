namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public interface ITenantUnitOfWork
	{
		void CancelAndDisposeCurrent();
		void CommitAndDisposeCurrent();
	}
}