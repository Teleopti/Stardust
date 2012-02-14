using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public sealed class DisableFilterScope : IDisposable
    {
    	private readonly NHibernateFilterManager _filterManager;
    	private readonly IQueryFilter _queryFilter;

        public DisableFilterScope(NHibernateFilterManager filterManager, IQueryFilter queryFilter)
        {
        	_filterManager = filterManager;
        	_queryFilter = queryFilter;
        }

        public void Dispose()
        {
            _filterManager.Enable(_queryFilter);
        }
    }
}
