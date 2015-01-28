using NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public interface ICurrentTenantSession
	{
		ISession CurrentSession();
	}
}