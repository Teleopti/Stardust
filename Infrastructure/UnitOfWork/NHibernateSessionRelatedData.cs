using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NHibernateSessionRelatedData
	{
		public NHibernateSessionRelatedData(NHibernateFilterManager filterManager, TransactionIsolationLevel isolationLevel)
		{
			FilterManager = filterManager;
			IsolationLevel = isolationLevel;
		}

		public NHibernateFilterManager FilterManager { get; private set; }

		public TransactionIsolationLevel IsolationLevel { get; private set; }
	}
}