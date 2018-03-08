using System;
using NHibernate.Impl;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public sealed class DisableFilterScope : IDisposable
    {
    	private readonly NHibernateFilterManager _filterManager;
    	private readonly IQueryFilter _queryFilter;
		private readonly FilterImpl _currentFilter;

	    public DisableFilterScope(NHibernateFilterManager filterManager, IQueryFilter queryFilter, FilterImpl currentFilter)
        {
        	_filterManager = filterManager;
        	_queryFilter = queryFilter;
			_currentFilter = currentFilter;
		}

	    public void Dispose()
		{
			if (_currentFilter != null)
				_filterManager.Enable(_queryFilter, _currentFilter.Parameters);
		}
    }
}
