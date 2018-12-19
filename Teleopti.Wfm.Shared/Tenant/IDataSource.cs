using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface IDataSource : IDisposable
    {
		IUnitOfWorkFactory Application { get; }
		IAnalyticsUnitOfWorkFactory Analytics { get; }
		IReadModelUnitOfWorkFactory ReadModel { get; }
    	string DataSourceName { get; }
        void RemoveAnalytics();
    }
}