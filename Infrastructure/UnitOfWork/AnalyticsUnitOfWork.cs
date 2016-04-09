using NHibernate;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class AnalyticsUnitOfWork : NHibernateUnitOfWork
	{
		public AnalyticsUnitOfWork(UnitOfWorkContext context, ISession session) : base(context, session, TransactionIsolationLevel.Default, null)
		{
		}
	}
}