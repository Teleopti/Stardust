using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
    public interface IDataSource : IDisposable
    {
		IUnitOfWorkFactory Application { get; }
		IUnitOfWorkFactory Statistic { get; }
		IReadModelUnitOfWorkFactory ReadModel { get; }
    	string DataSourceName { get; }
        void ResetStatistic();
		string OriginalFileName { get; set; }
		AuthenticationTypeOption AuthenticationTypeOption { get; set; }
    }
}