namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NHibernateSessionRelatedData
	{
		public NHibernateSessionRelatedData(NHibernateFilterManager filterManager)
		{
			FilterManager = filterManager;
		}

		public NHibernateFilterManager FilterManager { get; private set; }
	}
}