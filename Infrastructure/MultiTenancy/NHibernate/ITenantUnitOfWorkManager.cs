namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public interface ITenantUnitOfWorkManager
	{
		void CancelCurrent();
		void CommitCurrent();
	}
}