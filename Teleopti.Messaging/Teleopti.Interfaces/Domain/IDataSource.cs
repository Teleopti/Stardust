using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
    public interface IDataSource : IDisposable
    {
		IUnitOfWorkFactory Application { get; }
		IUnitOfWorkFactory Analytics { get; }
		IReadModelUnitOfWorkFactory ReadModel { get; }
    	string DataSourceName { get; }
        void ResetStatistic();
    }
}