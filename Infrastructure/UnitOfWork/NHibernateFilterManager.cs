using System;
using NHibernate;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NHibernateFilterManager
	{
		private readonly ISessionFactory _sessionFactory;
		private readonly DisabledFilterCounter _disabledFilterCounter;

		public NHibernateFilterManager(ISessionFactory sessionFactory)
		{
			_sessionFactory = sessionFactory;
			_disabledFilterCounter = new DisabledFilterCounter();
		}

		public IDisposable Disable(IQueryFilter filter)
		{
			_disabledFilterCounter.Increase(filter);
			var scope = new DisableFilterScope(this, filter);
			_sessionFactory.GetCurrentSession().DisableFilter(filter.Name);
			return scope;	
		}

		public void Enable(IQueryFilter filter)
		{
			if (_disabledFilterCounter.DecreaseAndCheckIfDisabled(filter))
			{
				_sessionFactory.GetCurrentSession().EnableFilter(filter.Name);				
			}
		}
	}
}