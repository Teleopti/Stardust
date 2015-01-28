using NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public interface ICurrentTennantSession
	{
		ISession CurrentSession();
	}
}