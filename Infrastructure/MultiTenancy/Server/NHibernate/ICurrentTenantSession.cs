using NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public interface ICurrentTenantSession
	{
		ISession CurrentSession();
	}
}