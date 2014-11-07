using System.Data.SqlClient;
using System.Linq;
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

		private string martConnectionString(string name)
		{
			var ds = _dataSourceProvider.RetrieveDatasourcesForApplication().ToList();
			if (ds.Count() == 1)
				return ds.First().Statistic.ConnectionString;
			foreach (var dataSource in ds)
			{
				if(dataSource.DataSourceName == name)
				return dataSource.Statistic.ConnectionString;
			}
			return "";
		}

		public SqlConnection MartConnection(string name, int latency)
		{
			var connectionString = martConnectionString(name);
			if (connectionString != "")
			{
				if (latency > 0)
					Thread.Sleep(latency);
				return new SqlConnection(connectionString);
			}
				
			return null;
		}
	}
}