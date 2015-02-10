using System.Data.SqlClient;
using System.Threading;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public interface IDatabaseConnectionHandler
	{
		SqlConnection MartConnection(string name, int latency);
	}

	public class DatabaseConnectionHandler : IDatabaseConnectionHandler
	{
		private readonly IDataSourcesProvider _dataSourceProvider;

		public DatabaseConnectionHandler(IDataSourcesProvider dataSourceProvider)
		{
			_dataSourceProvider = dataSourceProvider;
		}

		public SqlConnection MartConnection(string name, int latency)
		{
			if (latency > 0)
				Thread.Sleep(latency);
			return new SqlConnection(_dataSourceProvider.RetrieveDataSourceByName(name).Statistic.ConnectionString);
		}
	}
}