using System.Data.SqlClient;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public interface IDatabaseConnectionHandler
	{
		SqlConnection MartConnection(string name);
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

		public SqlConnection MartConnection(string name)
		{
			var connectionString = martConnectionString(name);
			if(connectionString != "")
				return new SqlConnection(connectionString);
			return null;
		}
	}
}