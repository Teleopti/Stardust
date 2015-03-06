namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public interface ITenantUnitOfWorkManager
	{
		void CancelCurrent();
		void CommitCurrent();
	}
}