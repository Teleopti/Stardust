using System.Data.SqlClient;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public interface IDatabaseConnectionHandler
	{
		SqlConnection MartConnection(string name, int latency);
	}

	public class DatabaseConnectionHandler : IDatabaseConnectionHandler
	{
		private readonly IDataSourceForTenant _dataSourceForTenant;

		public DatabaseConnectionHandler(IDataSourceForTenant dataSourceForTenant)
		{
			_dataSourceForTenant = dataSourceForTenant;
		}

		public SqlConnection MartConnection(string name, int latency)
		{
			if (latency > 0)
				Thread.Sleep(latency);
			return new SqlConnection(_dataSourceForTenant.Tenant(name).Statistic.ConnectionString);
		}
	}
}