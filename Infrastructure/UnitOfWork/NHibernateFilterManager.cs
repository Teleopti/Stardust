using System;
using NHibernate;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NHibernateFilterManager
	{
		private readonly ISession _session;
		private readonly DisabledFilterCounter _disabledFilterCounter;

		public NHibernateFilterManager(ISession session)
		{
			_session = session;
			_disabledFilterCounter = new DisabledFilterCounter();
		}

		public IDisposable Disable(IQueryFilter filter)
		{
			_disabledFilterCounter.Increase(filter);
			var scope = new DisableFilterScope(this, filter);
			_session.DisableFilter(filter.Name);
			return scope;	
		}

		public void Enable(IQueryFilter filter)
		{
			if (_disabledFilterCounter.DecreaseAndCheckIfDisabled(filter))
				_session.EnableFilter(filter.Name);
		}
	}
}