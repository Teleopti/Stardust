using NHibernate;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IHaveSession
	{
		ISession GetSession();
	}
}