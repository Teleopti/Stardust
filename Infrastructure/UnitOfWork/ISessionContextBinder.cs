using NHibernate;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface ISessionContextBinder
	{
		NHibernateFilterManager FilterManager(ISession session);
		TransactionIsolationLevel IsolationLevel(ISession session);
		void Bind(ISession session, TransactionIsolationLevel isolationLevel);
		void Unbind(ISession session);
	}
}