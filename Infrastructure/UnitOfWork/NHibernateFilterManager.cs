using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Impl;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NHibernateFilterManager
	{
		//would be much better to have a ref to ISesssionFactory here instead
		//feels a lot safer regarding memory leaks...
		//unfortunatly - it won't work 100% in desktop app currently 
		//where we have nested uows/ISessions
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
			var currentFilter = (FilterImpl)_session.GetEnabledFilter(filter.Name);
			var parameters= currentFilter==null ? new Dictionary<string, object>() : currentFilter.Parameters;
			var scope = new DisableFilterScope(this, filter, parameters);
			_session.DisableFilter(filter.Name);
			return scope;	
		}

		public void Enable(IQueryFilter filter, IDictionary<string, object> parameters)
		{
			if (_disabledFilterCounter.DecreaseAndCheckIfDisabled(filter))
			{
				var sessionFilter = _session.EnableFilter(filter.Name);
				foreach (var parameter in parameters)
				{
					sessionFilter.SetParameter(parameter.Key, parameter.Value);
				}
			}
		}
	}
}