using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public sealed class DisableFilterScope : IDisposable
    {
    	private readonly NHibernateFilterManager _filterManager;
    	private readonly IQueryFilter _queryFilter;
	    private readonly IDictionary<string, object> _parameters;

	    public DisableFilterScope(NHibernateFilterManager filterManager, IQueryFilter queryFilter, IDictionary<string, object> parameters)
        {
        	_filterManager = filterManager;
        	_queryFilter = queryFilter;
	        _parameters = parameters;
        }

	    public void Dispose()
        {
            _filterManager.Enable(_queryFilter, _parameters);
        }
    }
}
